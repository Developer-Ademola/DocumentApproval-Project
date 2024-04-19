using DocumentApproval_DataSource.Entities;
using DocumentApproval_DataSource.Repository.Interface;
using DocumentApproval_DTOs.RequestDTOs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;


namespace DocumentApproval_DataSource.Repository.Implementation
{
    public class CosmosDbSercice : ICosmosDbService
    {
        private readonly Container _container;
        private readonly Container _approveContainer;
        private readonly CosmosDbSetting _cosmosDbSettings;
        public CosmosDbSercice(IOptions<CosmosDbSetting> cosmosDbSettings)
        {
            _cosmosDbSettings = cosmosDbSettings.Value;

            var cosmosClient = new CosmosClient(_cosmosDbSettings.AccountEndpoint, _cosmosDbSettings.AccountKey);
            var database = cosmosClient.GetDatabase(_cosmosDbSettings.DatabaseName);
            _container = database.GetContainer("CreateRequest");
            _approveContainer = database.GetContainer("ApproveDocument");
        }

        public async Task<ApprovalRequest> ApproveRequest(ApprovalRequest ApproveRequest)
        {
            var response = await _approveContainer.CreateItemAsync(ApproveRequest);
            return response.Resource;
        }
        public async Task<CreateRequest> CreateRequest (CreateRequest request)
        {
            var response = await _container.CreateItemAsync(request);
            return response.Resource;
        }
        public async Task<CreateRequest> GetAll(CreateRequest request)
        {
            var query = "SELECT * FROM c";
            var iterator = _container.GetItemQueryIterator<CreateRequest>(new QueryDefinition(query));
            var items = new List<CreateRequest>();

            while (iterator.HasMoreResults)
            {
                var response = await iterator.ReadNextAsync();
                items.AddRange(response.ToList());
            }

            return request;
        }
        public async Task<List<ApprovalRequestDTOs>> GetAllApprovedDocumentByUserEmail(string targetEmail)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE (ARRAY_CONTAINS(c.Status, ' Approved') OR ARRAY_CONTAINS(c.Status, 'ApprovedAndSigned')) AND c.ApproverEmail = @targetEmail")
                  .WithParameter("@targetEmail", targetEmail);
                var document = new List<ApprovalRequestDTOs>();
                var iterator = _approveContainer.GetItemQueryIterator<ApprovalRequestDTOs>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    document.AddRange(response);
                }

                return document;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<ApprovalRequestDTOs>(); // No documents found
            }
            catch (Exception ex)
            {
                //Log or handle the exception as needed
                throw;
            }
        }
        public async Task<List<ApprovalDetailModel>> GetAllApproveDocumentByEmail(string documentName)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.DocumentName = @documentName")
                   .WithParameter("@documentName", documentName);
                var document = new List<ApprovalDetailModel>();
                var iterator = _approveContainer.GetItemQueryIterator<ApprovalDetailModel>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    document.AddRange(response);
                }

                return document;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<ApprovalDetailModel>(); // No documents found
            }
            catch (Exception ex)
            {
                //Log or handle the exception as needed
                throw;
            }
        }
        public async Task<List<ApprovalRequestDTOs>> GetAllRejectedDocumentByUserEmail(string targetEmail)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.ApproverEmail =  @targetEmail AND c.Status = Rejected")
                   .WithParameter("@targetEmail", targetEmail);
                var document = new List<ApprovalRequestDTOs>();
                var iterator = _approveContainer.GetItemQueryIterator<ApprovalRequestDTOs>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    document.AddRange(response);
                }

                return document;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<ApprovalRequestDTOs>(); // No documents found
            }
            catch (Exception ex)
            {
                //Log or handle the exception as needed
                throw;
            }
        }
        public async Task<List<CreateRequestDTOs>> GetApprovalEmailByUser(string targetEmail)
        {
            try
            {
                targetEmail = targetEmail.ToLower();

                var query = new QueryDefinition("SELECT * FROM c WHERE ARRAY_CONTAINS(c.Approvals, @targetEmail)")
                    .WithParameter("@targetEmail", targetEmail);

                var documents = new List<CreateRequestDTOs>();
                var iterator = _container.GetItemQueryIterator<CreateRequestDTOs>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    documents.AddRange(response);
                }

                return documents;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<CreateRequestDTOs>(); // No documents found
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw;
            }
        }
        public async Task<List<CreateRequestDTOs>> GetDocumentByDocumentNameandEmail(string targetEmail, string documentName)
        {
            try
            {
                targetEmail = targetEmail.ToLower();
                var query = new QueryDefinition("SELECT * FROM c WHERE ARRAY_CONTAINS(c.Approvals, @targetEmail) AND c.DocumentName = @documentName")
                            .WithParameter("@targetEmail", targetEmail)
                            .WithParameter("@documentName", documentName);
                var documents = new List<CreateRequestDTOs>();
                var iterator = _container.GetItemQueryIterator<CreateRequestDTOs>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    documents.AddRange(response);
                }

                return documents;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<CreateRequestDTOs>(); // No documents found
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw;
            }
        }
        public async Task<List<EmailModel>> GetDocumentByEmail(string userEmail, string documentName)
        {
            try
            {
                var query = new QueryDefinition("SELECT c.Approvals FROM c WHERE c.Email = @userEmail AND c.DocumentName = @documentName")
                    .WithParameter("@userEmail", userEmail)
                    .WithParameter("@documentName", documentName);

                var documents = new List<EmailModel>();
                var iterator = _container.GetItemQueryIterator<EmailModel>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    documents.AddRange(response);
                }

                return documents;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<EmailModel>(); // No documents found
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw;
            }
        }
        public async Task<List<CreateRequestDTOs>> GetDocumentByNameAsync(string targetEmail, string documentName)
        {
            try
            {
                targetEmail = targetEmail.ToLower();
                var query = new QueryDefinition("SELECT * FROM c WHERE ARRAY_CONTAINS(c.Approvals, @targetEmail) AND c.DocumentName = @documentName")
                            .WithParameter("@targetEmail", targetEmail)
                            .WithParameter("@documentName", documentName);
                var documents = new List<CreateRequestDTOs>();
                var iterator = _container.GetItemQueryIterator<CreateRequestDTOs>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    documents.AddRange(response);
                }

                return documents;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<CreateRequestDTOs>(); // No documents found
            }
            catch (Exception ex)
            {
                // Log or handle the exception as needed
                throw;
            }
        }
        public async Task DeleteItemAsync(string id)
        {
            await _container.DeleteItemAsync<CreateRequest>(id, new PartitionKey(id));
        }
        public async Task<List<ApprovalDetailModel>> GetAlreadyApproveDocumentByEmail(string documentName, string email)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.DocumentName = @documentName AND c.ApproverName = @email")
                   .WithParameter("@documentName", documentName)
                    .WithParameter("@userEmail", email);
                var document = new List<ApprovalDetailModel>();
                var iterator = _approveContainer.GetItemQueryIterator<ApprovalDetailModel>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    document.AddRange(response);
                }

                return document;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<ApprovalDetailModel>(); // No documents found
            }
            catch (Exception ex)
            {
                //Log or handle the exception as needed
                throw;
            }
        }
    }
}
