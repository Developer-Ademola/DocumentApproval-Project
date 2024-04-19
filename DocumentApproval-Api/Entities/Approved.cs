namespace DocumentApproval_Api.Entities
{
    public class Approved
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public string ApproverEmail { get; set; }
        public string ApproverName { get; set; }
       public string DocumentName { get; set; }
        public string Status { get; set; }
        public string DateApproved { get; set; }
        public string TimeApproved { get; set; }
      
    }
}
