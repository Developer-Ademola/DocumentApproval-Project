
using DocumentApproval_Api.Interfaces;
using DocumentApproval_DataSource.Entities;
using DocumentApproval_DTOs.RequestDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using System.Security.Claims;

namespace DocumentApproval_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreateRequestController : ControllerBase
    {
        private readonly IStorageLake _azureStorage;
        private readonly ICosmosDbService _cosmosDbService;
        private readonly IEmailSender _emailService;
        public string ApproveStatus = "Pending";
        public CreateRequestController(IStorageLake azureStorage, ICosmosDbService cosmosDbService, IEmailSender emailService)
        {
            _azureStorage = azureStorage;
            _cosmosDbService = cosmosDbService;
            _emailService = emailService;
        }
       
        [HttpPost("CreateRequest")]
        public async Task<IActionResult> CreateRequest([FromForm] CreateRequest request)
        {
            var userClaimsIdentity = User.Identity as ClaimsIdentity;
            try
            {
                var fullName = userClaimsIdentity.FindFirst("name");
                var userName = fullName?.Value;
                var email = userClaimsIdentity.FindFirst("preferred_username");
                var userEmail = email?.Value;
                // Validate the request
                if (request == null)
                    return BadRequest("Invalid request. Please provide the required data.");

                // Validate the uploaded file
                var uploadedFile = request.DocumentFile;
                if (uploadedFile == null || uploadedFile.Length == 0)
                    return BadRequest("No file uploaded.");

                // Generate a unique file name
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(uploadedFile.FileName);
                //Original File
                string Oringinalfile = uploadedFile.FileName;
                // Upload document to Azure Data Lake Storage
                var uploadResult = await _azureStorage.UploadAsync(uploadedFile);

                // Create a new DocumentRequest instance
                var docRequest = new CreateRequest
                {
                    FullName = userName,
                    Email = userEmail,
                    DocumentName = Oringinalfile,
                    DateCreated = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(5), // Set default due date
                    DocumentCategory = request.DocumentCategory,
                    Approvals = request.Approvals,
                    ApprovalStatus = request.ApprovalStatus,
                    ApprovedType = request.ApprovedType,
                    DocumentDescription = request.DocumentDescription
                };

                // Save request to Azure Cosmos DB
                await _cosmosDbService.CreateRequest(docRequest);
                await _emailService.SendDocumentApprovalRequestAsync(request.Approvals, userName, Oringinalfile);

                // Send email notification
                return Ok("Request created and document uploaded successfully.");
            }
            catch (Exception ex)
            {
                // Handle errors
                // If saving to Cosmos DB fails, delete the uploaded document from Azure Data Lake Storage
                await _azureStorage.DeleteAsync(request.DocumentName); // Ensure to provide the correct document name
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }

        [HttpGet("AllRequest")]
        public async Task<IActionResult> AllRequestCreatedByEmail()
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
                var emailAddress = await _cosmosDbService.GetAllRequestCreatedByUserMail(userEmail);
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

                return Ok(documentDtos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }
    }
}
