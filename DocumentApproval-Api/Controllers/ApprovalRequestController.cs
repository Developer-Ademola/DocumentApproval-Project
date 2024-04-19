using Azure.Core;
using DocumentApproval_Api.Entities;
using DocumentApproval_Api.Interfaces;
using DocumentApproval_Api.RequestDTOs;
using DocumentApproval_DataSource.Entities;
using DocumentApproval_DTOs.RequestDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static DocumentApproval_DataSource.Entities.ApprovalRequest;

namespace DocumentApproval_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApprovalRequestController : ControllerBase
    {
        private readonly IStorageLake _azureStorage;
        private readonly ICosmosDbService _cosmosDbService;
        private readonly IEmailSender _emailService;

        public ApprovalRequestController(IStorageLake azureStorage, ICosmosDbService cosmosDbService, IEmailSender emailService)
        {
            _azureStorage = azureStorage;
            _cosmosDbService = cosmosDbService;
            _emailService = emailService;
        }
        [HttpGet("Inbox")]
        public async Task<IActionResult> GetDocumentAllDocumentRequest()
        {
            string ApprovalStatus = "Pending";
            var userClaimsIdentity = User.Identity as ClaimsIdentity;
            try
            {
                var fullName = userClaimsIdentity.FindFirst("name");
                var userName = fullName?.Value?.ToLower();
                var email = userClaimsIdentity.FindFirst("preferred_username");
                var userEmail = email?.Value?.ToLower();

                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest("Email claim not found in the token.");
                }

                // Retrieve the document request from Cosmos DB based on the user's email
                var emailAddress = await _cosmosDbService.GetApprovalEmailByUser(userEmail);
                if (emailAddress == null)
                {
                    return NotFound($"No documents found for email '{userEmail}'.");
                }

                // Filter out other people's email from the Approvals list
                var documentDtos = emailAddress.Select(document => new CreateRequestDTOs
                {
                    Id = document.Id,
                    FullName = document.FullName,
                    Email = document.Email,
                    DocumentName = document.DocumentName,
                    DateCreated = document.DateCreated,
                    DueDate = document.DueDate,
                    DocumentCategory = document.DocumentCategory,
                    Approvals = document.Approvals,
                    ApprovedType = document.ApprovedType,
                    DocumentDescription = document.DocumentDescription,
                    ApprovalStatus = document.ApprovalStatus,
                    // Map other fields you want to include
                }).ToList();

                // Filter out approvals not belonging to the current user
                foreach (var document in documentDtos)
                {
                    document.Approvals.RemoveAll(approvalEmail => approvalEmail != userEmail);
                }

                return Ok(documentDtos);
            }
            catch (Exception ex)
            {
                return BadRequest ($"An error occurred: {ex.Message}");
            }
        }
        [HttpPost("{Id}")]
        public async Task<IActionResult> SearchDocumentRequest(Guid Id, [FromForm] ApprovalRequest request)
        {
            var userClaimsIdentity = User.Identity as ClaimsIdentity;
            var fullName = userClaimsIdentity.FindFirst("name");
            var userName = fullName?.Value?.ToLower();
            var email = userClaimsIdentity.FindFirst("preferred_username");
            var userEmail = email?.Value?.ToLower();

            if (string.IsNullOrEmpty(userEmail))
            {
                return BadRequest("Email claim not found in the token.");
            }
            if (request == null)
            {
                return BadRequest("Invalid request. Please provide the required data.");
            }
            // Validate the uploaded file
            var uploadedFile = request.DocumentFile;
            if (uploadedFile == null || uploadedFile.Length == 0)
            {
                return BadRequest("No file uploaded.");
            }

            // Generate a unique file name
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedFile.FileName);
            // Original File
            string OriginalFile = uploadedFile.FileName;
            // Upload document to Azure Data Lake Storage
            var uploadResult = await _azureStorage.UploadAsync(uploadedFile);

            // Retrieve the document request from Cosmos DB based on the user's email
            var emailAddress = await _cosmosDbService.GetDocumentsAndEmailAsync(userEmail, Id.ToString());
            if (emailAddress == null)
            {
                return NotFound($"No documents found for email '{userEmail}'.");
            }

            try
            {
                var documentDtos = new List<CreateRequestDTOs>();
                foreach (var document in emailAddress)
                {
                    var approvalDto = new CreateRequestDTOs
                    {
                        // Filter out other people's email from the Approvals list

                        FullName = document.FullName,
                        Email = document.Email,
                        DocumentName = document.DocumentName,
                        DateCreated = document.DateCreated,
                        DueDate = document.DueDate,
                        DocumentCategory = document.DocumentCategory,
                        Approvals = document.Approvals,
                        ApprovalStatus = document.ApprovalStatus,
                        ApprovedType = document.ApprovedType,
                        DocumentDescription = document.DocumentDescription
                    };
                    documentDtos.Add(approvalDto);

                    var docRequest = new ApprovalRequest
                    {
                        Id = request.Id, // Use the Id from Cosmos DB
                        FullName = document.FullName,
                        Email = document.Email,
                        DocumentName = document.DocumentName,
                        DateCreated = document.DateCreated,
                        DueDate = document.DueDate,
                        RequestId = document.Id,
                        DocumentCategory = document.DocumentCategory,
                        ApprovedType = document.ApprovedType,
                        DocumentDescription = document.DocumentDescription,
                        ApproverName = userName,
                        ApproverEmail = userEmail,
                        UploadDocumentName = OriginalFile,
                        Status = request.Status,
                        DateApproved = DateTime.UtcNow.Date,
                        TimeApproved = DateTime.UtcNow.TimeOfDay
                    };
                    var result = await _cosmosDbService.AddApprovalItemAsync(docRequest);
                }

                foreach (var document in documentDtos)
                {
                    document.Approvals.RemoveAll(approvalEmail => approvalEmail != userEmail);
                }

                foreach (var document in documentDtos)
                {
                    // Assuming you have the logic for sending email response based on document details
                    await _emailService.SendDocumentApprovalResponseAsync(document.Email, userName, document.DocumentName);
                }

                return Ok("Document Approved Created and document uploaded successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }


        //public async Task<IActionResult> ApproveDocument(string documentName, [FromForm] ApprovalRequest request)
        //{
        //    var userClaimsIdentity = User.Identity as ClaimsIdentity;
        //    var fullName = userClaimsIdentity.FindFirst("name")?.Value;
        //    var userName = !string.IsNullOrEmpty(fullName) ? fullName.ToLower() : null;
        //    var email = userClaimsIdentity.FindFirst("preferred_username")?.Value;
        //    var userEmail = !string.IsNullOrEmpty(email) ? email.ToLower() : null;

        //    // Check if user claims are valid
        //    if (string.IsNullOrEmpty(userEmail))
        //    {
        //        return BadRequest("Email claim not found in the token.");
        //    }

        //    // Validate the request
        //    if (request == null)
        //    {
        //        return BadRequest("Invalid request. Please provide the required data.");
        //    }

        //    // Validate the uploaded file
        //    var uploadedFile = request.DocumentFile;
        //    if (uploadedFile == null || uploadedFile.Length == 0)
        //    {
        //        return BadRequest("No file uploaded.");
        //    }

        //    try
        //    {
        //        // Generate a unique file name
        //        string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedFile.FileName);
        //        // Original File
        //        string originalFileName = uploadedFile.FileName;

        //        // Upload document to Azure Data Lake Storage
        //        var uploadResult = await _azureStorage.UploadAsync(uploadedFile);

        //        // Retrieve the document request from Cosmos DB based on the user's email and document name
        //        var documentRequests = await _cosmosDbService.GetDocumentsAndEmailAsync(userEmail, documentName);
        //        if (documentRequests == null || documentRequests.Count == 0)
        //        {
        //            return NotFound($"No documents found for email '{userEmail}' and document name '{documentName}'.");
        //        }

        //        foreach (var documentRequest in documentRequests)
        //        {
        //            // Approve the document request
        //            documentRequest.Approvals.RemoveAll(approvalEmail => approvalEmail != userEmail);
        //            documentRequest.Status = request.Status;
        //            documentRequest.DateApproved = DateTime.UtcNow.Date;
        //            documentRequest.TimeApproved = DateTime.UtcNow.TimeOfDay;

        //            // Update the document request in Cosmos DB
        //            var result = await _cosmosDbService.UpdateDocumentRequestAsync(documentRequest);

        //            // Send email response based on document details
        //            await _emailService.SendDocumentApprovalResponseAsync(documentRequest.Email, userName, documentRequest.DocumentName);
        //        }

        //        return Ok("Document approved and uploaded successfully.");
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest(ex.Message);
        //    }
        //}


        [HttpGet("ApprovedDocument")]
        public async Task<IActionResult> ApprovedDocument()
        {
            var userClaimsIdentity = User.Identity as ClaimsIdentity;
            try
            {
                var fullName = userClaimsIdentity.FindFirst("name");
                var userName = fullName?.Value?.ToLower();
                var email = userClaimsIdentity.FindFirst("preferred_username");
                var userEmail = email?.Value?.ToLower();
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest("Email claim not found in the token.");
                }

                // Retrieve the document request from Cosmos DB based on the user's email
                var emailAddress = await _cosmosDbService.GetAllApprovedDocumentByUserEmail(userEmail);
                if (emailAddress == null)
                {
                    return NotFound($"No documents found for email '{userEmail}'.");
                }

                // Filter out other people's email from the Approvals list
                var documentDtos = emailAddress.Select(document => new Approved
                {
                    Id = document.Id,
                    Email = document.Email,
                    ApproverEmail = userEmail,
                    ApproverName = document.ApproverName,
                    DocumentName = document.DocumentName,
                    Status = document.Status,
                    DateApproved = document.DateApproved,
                    TimeApproved = document.TimeApproved
                    // Map other fields you want to include
                }).ToList();

                // Filter out approvals not belonging to the current user
                return Ok(documentDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
        [HttpGet("RejectDocument")]
        public async Task<IActionResult> RejectDocument()
        {
            var userClaimsIdentity = User.Identity as ClaimsIdentity;
            try
            {
                var fullName = userClaimsIdentity.FindFirst("name");
                var userName = fullName?.Value?.ToLower();
                var email = userClaimsIdentity.FindFirst("preferred_username");
                var userEmail = email?.Value?.ToLower();
                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest("Email claim not found in the token.");
                }

                // Retrieve the document request from Cosmos DB based on the user's email
                var emailAddress = await _cosmosDbService.GetAllRejectedDocumentByUserEmail(userEmail);
                if (emailAddress == null)
                {
                    return NotFound($"No documents found for email '{userEmail}'.");
                }

                // Filter out other people's email from the Approvals list
                var documentDtos = emailAddress.Select(document => new Approved
                {
                    Id = document.Id,
                    ApproverEmail = userEmail,
                    ApproverName = document.ApproverName,
                    Email = document.Email,
                    DocumentName = document.DocumentName,
                    Status = document.Status,
                    DateApproved = document.DateApproved,
                    TimeApproved = document.TimeApproved
                    // Map other fields you want to include
                }).ToList();

                // Filter out approvals not belonging to the current user
                return Ok(documentDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

    }
}
