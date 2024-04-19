﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentApproval_Api.Interfaces
{
    public interface IEmailSender
    {
        Task SendDocumentApprovalRequestAsync(List<string> approvers, string senderName, string documentName);
        Task SendDocumentApprovalResponseAsync(string approver, string senderName, string documentName);
        Task SendDocumentRejectResponseAsync(string approver, string senderName, string documentName);
    }
}
