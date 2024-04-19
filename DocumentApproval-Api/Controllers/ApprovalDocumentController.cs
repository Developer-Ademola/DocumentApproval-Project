using DocumentApproval_Api.Interfaces;
using DocumentApproval_DTOs.RequestDTOs;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text;

namespace DocumentApproval_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApprovalDocumentController : ControllerBase
    {
        private readonly IStorageLake _azureStorage;
        private readonly ICosmosDbService _cosmosDbService;
        private readonly IEmailSender _emailService;

        public ApprovalDocumentController(IStorageLake azureStorage, ICosmosDbService cosmosDbService, IEmailSender emailService)
        {
            _azureStorage = azureStorage;
            _cosmosDbService = cosmosDbService;
            _emailService = emailService;
        }

        [HttpGet()]
        public async Task<IActionResult> GetAllApprovalStatus(Guid Id)
        {
            var userClaimsIdentity = User.Identity as ClaimsIdentity;
            try
            {
                var emailClaim = userClaimsIdentity?.FindFirst("preferred_username");
                var userEmail = emailClaim?.Value?.ToLower();

                if (string.IsNullOrEmpty(userEmail))
                {
                    return BadRequest("Email claim not found in the token.");
                }

                // Retrieve the document request from Cosmos DB based on the user's email and document name
                var emailAddress = await _cosmosDbService.GetDocumentByNameAsync(userEmail, Id);
                if (emailAddress == null)
                {
                    return NotFound($"No documents found for email '{userEmail}'.");
                }

                var approvalDetails = new StringBuilder();

                // Fetch all approval details for the current document
                var approvalDocuments = await _cosmosDbService.GetAllApproveDocumentByEmail(userEmail, Id);

                // Check if all approvals are in 'ApproveAndSigned' status
                bool allApprovedAndSigned = emailAddress.All(document => document.Approvals.All(email =>
                    approvalDocuments.Any(ad => ad.ApproverEmail.Equals(email, StringComparison.OrdinalIgnoreCase) && ad.Status.Contains("Ap"))));

                // Iterate through each email in the document approvals
                foreach (var document in emailAddress)
                {
                    foreach (var email in document.Approvals)
                    {
                        var userApprovalDocuments = approvalDocuments.Where(ad =>
                            ad.ApproverEmail.Equals(email, StringComparison.OrdinalIgnoreCase));

                        // Check if there are no approval documents for the current user
                        if (!userApprovalDocuments.Any())
                        {
                            // If no approvals found, append the email with null status
                            approvalDetails.AppendLine($"{email.Trim()} {{");
                            approvalDetails.AppendLine($"\tStatus: null,");
                            approvalDetails.AppendLine($"\tApproveDate: null,");
                            approvalDetails.AppendLine($"\tApproveTime: null");
                            approvalDetails.AppendLine("}");
                        }
                        else
                        {
                            // Append the approval details to the response string
                            foreach (var approvalDetail in userApprovalDocuments)
                            {
                                approvalDetails.AppendLine($"{email.Trim()} {{");
                                approvalDetails.AppendLine($"\tStatus: {approvalDetail.Status},");
                                approvalDetails.AppendLine($"\tApproveDate: {approvalDetail.DateApproved.ToShortDateString()},");
                                approvalDetails.AppendLine($"\tApproveTime: {approvalDetail.TimeApproved}");
                                approvalDetails.AppendLine("}");
                            }
                        }
                    }
                }

                var documentsToUpdate = await _cosmosDbService.UpdateDocumentAsync(userEmail, Id);
                foreach (var document in documentsToUpdate)
                {
                    document.ApprovalStatus = "Approved";
                }

                // Convert StringBuilder to string
                var approvalDetailsString = approvalDetails.ToString();

                var response = new
                {
                    ApprovalDetails = approvalDetailsString,
                    UpdatedDocuments = documentsToUpdate
                };

                if (allApprovedAndSigned)
                {
                    return Ok(new { Response = response, Message = "Approved" });
                }
                else
                {
                    return Ok(new { Response = response, Message = "Not all approvals are in 'ApproveAndSigned' status." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"An error occurred: {ex.Message}");
            }
        }




        //public async Task<IActionResult> GetAllApprovalStatus(Guid Id)
        //{
        //    var userClaimsIdentity = User.Identity as ClaimsIdentity;
        //    try
        //    {
        //        var emailClaim = userClaimsIdentity?.FindFirst("preferred_username");
        //        var userEmail = emailClaim?.Value?.ToLower();

        //        if (string.IsNullOrEmpty(userEmail))
        //        {
        //            return BadRequest("Email claim not found in the token.");
        //        }

        //        // Retrieve the document request from Cosmos DB based on the user's email and document name
        //        var emailAddress = await _cosmosDbService.GetDocumentByNameAsync(userEmail, Id);
        //        if (emailAddress == null)
        //        {
        //            return NotFound($"No documents found for email '{userEmail}'.");
        //        }

        //        // Fetch all approval details for the current document
        //        var approvalDocuments = await _cosmosDbService.GetAllApproveDocumentByEmail(userEmail, Id);

        //        // Check if all approval statuses are "ApproveAndSigned"
        //        bool allApprovedAndSigned = approvalDocuments.All(ad => ad.Status.Contains("Ap") || ad.Status.Contains("ApprovedAndSigned"));

        //        if (allApprovedAndSigned)
        //        {
        //            return Ok("All approvals are in 'ApproveAndSigned' status.");
        //        }
        //        else
        //        {
        //            return Ok("Not all approvals are in 'ApproveAndSigned' status.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"An error occurred: {ex.Message}");
        //    }
        //}



        //public async Task<IActionResult> GetAllApprovalStatus(Guid Id)
        //{
        //    var userClaimsIdentity = User.Identity as ClaimsIdentity;
        //    try
        //    {
        //        var emailClaim = userClaimsIdentity?.FindFirst("preferred_username");
        //        var userEmail = emailClaim?.Value?.ToLower();

        //        if (string.IsNullOrEmpty(userEmail))
        //        {
        //            return BadRequest("Email claim not found in the token.");
        //        }

        //        // Retrieve the document and approval details from Cosmos DB
        //        var approvalDetails = await _cosmosDbService.GetAllApproveDocumentByEmail(userEmail, Id);

        //        if (approvalDetails == null || !approvalDetails.Any())
        //        {
        //            return NotFound($"No documents found for email '{userEmail}' and document Id '{Id}'.");
        //        }

        //        // Prepare structured response data
        //        var responseData = approvalDetails.Select(ad => new
        //        {
        //            ApproverEmail = ad.ApproverEmail,
        //            Status = ad.Status ?? "null",
        //            ApproveDate = ad.DateApproved.ToShortDateString() ?? "null",
        //            ApproveTime = ad.TimeApproved.ToString() ?? "null"
        //        });

        //        return Ok(responseData);
        //    }
        //    catch (Exception ex)
        //    {
        //        return BadRequest($"An error occurred: {ex.Message}");
        //    }
        //}

        // [HttpPut("UpdateDocumentStatus")]
        //public async Task<IActionResult> UpdateRequestStatus(List<CreateRequestDTOs> documents, string Id)
        //{
        //    var userClaimsIdentity = User.Identity as ClaimsIdentity;
        //    try
        //    {
        //        var fullName = userClaimsIdentity.FindFirst("name");
        //        var userName = fullName?.Value?.ToLower();
        //        var email = userClaimsIdentity.FindFirst("preferred_username");
        //        var userEmail = email?.Value?.ToLower();
        //        if (string.IsNullOrEmpty(userEmail))
        //        {
        //            return BadRequest("Email claim not found in the token.");
        //        }

        //        // Retrieve the document request from Cosmos DB based on the user's email
        //        var emailAddress = await _cosmosDbService.UpdateApproveDocumentByEmail(userEmail, Id);
        //        if (emailAddress == null)
        //        {
        //            return NotFound($"No documents found for email '{userEmail}'.");
        //        }
        //        foreach (var document in documents)
        //        {
        //            // Update the Status property to "Approved"
        //            document.ApprovalStatus = "Approved";

        //            // Call your Cosmos DB update method here
        //            await _container.ReplaceItemAsync(document, document.Id.ToString(), new PartitionKey(document.PartitionKey));
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }   
        //}
    }
}
