using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentApproval_DataSource.Entities
{
    public class DownloadAndRetrieve
    {
        public CreateRequest DocumentRequest { get; set; }
        public string? Uri { get; set; }
        public string Name { get; set; }
        public string? ContentType { get; set; }
        public Stream? Content { get; set; }
    }
}
