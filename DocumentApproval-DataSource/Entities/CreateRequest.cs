using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentApproval_DataSource.Entities
{
    public class CreateRequest
    {
        [JsonIgnore]
        public Guid Id = Guid.NewGuid();
        public string FullName = null;
        public string Email = null;
        public string DocumentName = null;
        [JsonIgnore]
        public IFormFile DocumentFile { get; set; } // Represents the uploaded document file

        public DateTime DateCreated = DateTime.Now;

        public DateTime DueDate = DateTime.Now.AddDays(5);
        public string DocumentCategory { get; set; }
        public List<string> Approvals { get; set; }
        [JsonIgnore]
        public string ApprovalStatus = "Pending";
        public string ApprovedType { get; set; }
        public string DocumentDescription { get; set; }
    }
   
}
