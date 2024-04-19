using Newtonsoft.Json;

namespace DocumentApproval_Api.RequestDTOs
{
    public class ApprovalDto
    {
        [JsonProperty("Id")]
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; }
        public string Email { get; set; }
        public string DocumentName { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DueDate { get; set; }
        public string DocumentCategory { get; set; }
        public string ApprovedType { get; set; }
        public string DocumentDescription { get; set; }
        public List<string> Approvals { get; set; }
    }
}
