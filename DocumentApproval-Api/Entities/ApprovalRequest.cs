

using Newtonsoft.Json;

namespace DocumentApproval_DataSource.Entities
{
    public class ApprovalRequest
    {
        [JsonProperty(PropertyName = "id")]

        public Guid Id = Guid.NewGuid();
        [JsonIgnore]

        public string FullName;
        public string Email;
        public string DocumentName;
        public DateTime? DateCreated;
        public DateTime? DueDate;
        public Guid RequestId;
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
