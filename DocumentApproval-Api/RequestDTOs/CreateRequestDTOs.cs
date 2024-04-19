using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentApproval_DTOs.RequestDTOs
{
    public class CreateRequestDTOs
    {
        [JsonProperty(PropertyName = "id")]

        public Guid Id = Guid.NewGuid();
        [JsonIgnore]
        public string FullName { get; set; }
        public string Email { get; set; }
        public string DocumentName { get; set; }
        public DateTime? DateCreated { get; set; }
        public DateTime? DueDate { get; set; }
        public string DocumentCategory { get; set; }
        public List<string> Approvals { get; set; }
        public string ApprovalStatus { get; set; }
        public string ApprovedType { get; set; }
        public string DocumentDescription { get; set; }
        //public DateTime? DateApproved { get; set; }
        //public TimeSpan? TimeApproved { get; set; }


    }
}
