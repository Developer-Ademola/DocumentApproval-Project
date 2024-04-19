using Azure;
using Azure.Communication.Email;
using DocumentApproval_BusinessLogic.Implementation.Interfaces;
using javax.security.auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentApproval_BusinessLogic.Implementation.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailClient _emailClient;

        public EmailSender(IConfiguration configuration)
        {
            var connectionString = configuration["EmailSettings:ConnectionString"];
            _emailClient = new EmailClient(connectionString);
        }

        public async Task SendDocumentApprovalRequestAsync(List<string> approvers, string senderName, string documentName)
        {
            var sender = "donotreply@69973990-cc6f-41ec-a72a-d3ecd53c4816.azurecomm.net"; // Change this
            var subject = "Document Approval Request";

            foreach (var approver in approvers)
            {
                var htmlContent = $"<html>This is to notify you that {senderName} requested your approval for the document '{documentName}'.<br>" +
                                  $"Please click <a href=\"#\">here</a> to approve.</html>";

                await _emailClient.SendAsync(
                    wait: WaitUntil.Completed,
                    senderAddress: sender,
                    recipientAddress: approver,
                    subject: subject,
                    htmlContent: htmlContent);
            }
        }
           

        public async Task SendDocumentApprovalResponseAsync(string approver, string senderName, string documentName)
        {
            var sender = "donotreply@69973990-cc6f-41ec-a72a-d3ecd53c4816.azurecomm.net"; // Change this
            var subject = "Document Approval Request";
            var htmlContent = $"<html>This is to notify you that the document '{documentName}' sent to {senderName} has been approved.<br>" +
                              "Best Regards</html>";

            await _emailClient.SendAsync(
                wait: WaitUntil.Completed,
                senderAddress: sender,
                recipientAddress: approver,
                subject: subject,
                htmlContent: htmlContent);
        
    }
    }
}
