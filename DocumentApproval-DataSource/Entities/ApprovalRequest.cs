using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentApproval_DataSource.Entities
{
    public class ApprovalRequest
    {
        [JsonProperty("id")]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName;
        public string Email;
        public string DocumentName;
        public DateTime DateCreated;
        public DateTime DueDate;
        public string DocumentCategory;
        public string ApprovedType;
        public string DocumentDescription;
        public string ApproverName;
        public string ApproverEmail;
        [JsonIgnore]
        public IFormFile DocumentFile { get; set; } // Represents the uploaded document file
        public string UploadDocumentName;
        public ApprovalStatus Status { get; set; }
        public DateTime? DateApproved;
        public TimeSpan? TimeApproved;
    }

    public enum ApprovalStatus
    {
        Approved,
        ApprovedAndSigned,
        Rejected
    }

}
