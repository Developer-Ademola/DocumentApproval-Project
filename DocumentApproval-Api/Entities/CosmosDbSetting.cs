using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentApproval_DataSource.Entities
{
    public class CosmosDbSetting
    {
        public string AccountEndpoint { get; set; }
        public string AccountKey { get; set; }
        public string DatabaseName { get; set; }
    }
}
