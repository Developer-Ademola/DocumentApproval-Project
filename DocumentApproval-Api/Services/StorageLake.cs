

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DocumentApproval_Api.Interfaces;
using DocumentApproval_DataSource.Entities;

namespace DocumentApproval_Api.Services
{
    public class StorageLake : IStorageLake
    {
        private readonly string _storageConnectionString;
        private readonly string _storageContainerName;
        private readonly ILogger<StorageLake> _logger;
        private readonly BlobContainerClient _containerClient;
        public StorageLake(IConfiguration configuration, ILogger<StorageLake> logger)
        {
            _storageConnectionString = configuration.GetValue<string>("BlobConnectionString");
            _storageContainerName = configuration.GetValue<string>("BlobContainerName");
            _logger = logger;
            _containerClient = new BlobContainerClient(_storageConnectionString, _storageContainerName);
        }
        public async Task<CreateRequest> DeleteAsync(string blobFilename)
        {
            BlobClient blobClient = _containerClient.GetBlobClient(blobFilename);

            try
            {
                await blobClient.DeleteAsync();
                return null;
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                _logger.LogError($"File {blobFilename} was not found");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while deleting blob {blobFilename}: {ex.Message}");
                return null;
            }
        }

        public async Task<DownloadAndRetrieve> DownloadAsync(string blobFilename)
        {
            BlobClient blobClient = _containerClient.GetBlobClient(blobFilename);

            try
            {
                if (await blobClient.ExistsAsync())
                {
                    var data = await blobClient.OpenReadAsync();
                    Stream blobContent = data;

                    var downloadResult = await blobClient.DownloadAsync();

                    string name = blobFilename;
                    string contentType = downloadResult.Value.ContentType;

                    return new DownloadAndRetrieve { Content = blobContent, Name = name, ContentType = contentType };
                }
                else
                {
                    _logger.LogError($"File {blobFilename} was not found");
                    return null;
                }
            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobNotFound)
            {
                _logger.LogError($"File {blobFilename} was not found");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError($"An error occurred while downloading blob {blobFilename}: {ex.Message}");
                return null;
            }
        }

        public async Task<CreateRequest> UploadAsync(IFormFile file)
        {
            CreateRequest response = new CreateRequest();

            try
            {
                BlobClient blobClient = _containerClient.GetBlobClient(file.FileName);

                await using (Stream data = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(data);
                }

                response.DocumentName = blobClient.Name;

            }
            catch (RequestFailedException ex) when (ex.ErrorCode == BlobErrorCode.BlobAlreadyExists)
            {
                _logger.LogError($"File with name {file.FileName} already exists in container: '{_storageContainerName}'.");

            }
            catch (Exception ex)
            {
                _logger.LogError($"Unhandled Exception. ID: {ex.StackTrace} - Message: {ex.Message}");

            }

            return response;
        }
    }
}
