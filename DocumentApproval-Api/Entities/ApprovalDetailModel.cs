using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentApproval_DataSource.Entities
{
    public class ApprovalDetailModel
    {
        public string ApproverEmail { get; set; }
        public string Status { get; set; }
        public DateTime DateApproved { get; set; }
        public TimeSpan TimeApproved { get; set; }
    }
}
