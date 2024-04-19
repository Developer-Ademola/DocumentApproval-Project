

using DocumentApproval_DataSource.Entities;

namespace DocumentApproval_Api.Interfaces
{
    public interface IStorageLake
    {
        Task<CreateRequest> UploadAsync(IFormFile file);
        Task<DownloadAndRetrieve> DownloadAsync(string documentName);
        // {> DownloadAsync(string documentName);
        Task<CreateRequest> DeleteAsync(string blobFilename);
    }
}
