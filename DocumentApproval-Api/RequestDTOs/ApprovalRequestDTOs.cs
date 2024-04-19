using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentApproval_DTOs.RequestDTOs
{
    public class ApprovalRequestDTOs
    {
        [JsonProperty("id")]
        public Guid Id = Guid.NewGuid();
        public string FullName;
        public string Email;
        public string DocumentName;
        public DateTime DateCreated { get; set; }
        public DateTime DueDate { get; set; }
        public string DocumentCategory;
        public string ApprovedType;
        public string DocumentDescription;
        public string ApproverName;
        public string ApproverEmail;
        [JsonIgnore]
        public IFormFile DocumentFile { get; set; } // Represents the uploaded document file
        public string UploadDocumentName;
        public string Status { get; set; }
        public DateTime? DateApproved;
        public TimeSpan? TimeApproved;

    }
}
