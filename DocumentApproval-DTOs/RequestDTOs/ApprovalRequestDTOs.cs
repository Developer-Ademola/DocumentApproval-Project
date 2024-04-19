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
        public Guid Id { get; set; } = Guid.NewGuid();
        public string ApproverEmail { get; set; }
        public string ApproverName { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string DocumentName { get; set; }
        public string Status { get; set; }
        public DateTime? DateApproved { get; set; }
        public TimeSpan? TimeApproved { get; set; }
    }
}
