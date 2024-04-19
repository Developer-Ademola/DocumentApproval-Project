using DocumentApproval_DataSource.Entities;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentApproval_BusinessLogic.Implementation.Interfaces
{
    public interface IStorageLake
    {
        Task<CreateRequest> UploadAsync(IFormFile file);
        Task<DownloadAndRetrieve> DownloadAsync(string documentName);
        // {> DownloadAsync(string documentName);
        Task<CreateRequest> DeleteAsync(string blobFilename);
    }
}
