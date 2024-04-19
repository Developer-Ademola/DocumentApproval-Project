using DocumentApproval_Api.Entities;
using DocumentApproval_Api.Interfaces;
using DocumentApproval_Api.RequestDTOs;
using DocumentApproval_DataSource.Entities;
using DocumentApproval_DTOs.RequestDTOs;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using System.Net;
using System.Reflection.Metadata;


namespace DocumentApproval_Api.Services
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
        public async Task<ApprovalRequest> AddApprovalItemAsync(ApprovalRequest item)
        {
            var response = await _approveContainer.CreateItemAsync(item);
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
        public async Task<List<Approved>> GetAllApprovedDocumentByUserEmail(string targetEmail)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE (CONTAINS(LOWER(c.Status), 'ap')) AND c.ApproverEmail = @targetEmail")
                    .WithParameter("@targetEmail", targetEmail);
                var document = new List<Approved>();
                var iterator = _approveContainer.GetItemQueryIterator<Approved>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    document.AddRange(response);
                }

                return document;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<Approved>(); // No documents found
            }
            catch (Exception ex)
            {
                //Log or handle the exception as needed
                throw;
            }
        }
        public async Task<List<ApprovalDetailModel>> GetAllApproveDocumentByEmail(string targetEmail, Guid Id)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.RequestId = @Id AND LOWER(c.Email) = @targetEmail")
                            .WithParameter("@Id", Id)
                            .WithParameter("@targetEmail", targetEmail);

                var documents = new List<ApprovalDetailModel>();
                var iterator = _approveContainer.GetItemQueryIterator<ApprovalDetailModel>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    documents.AddRange(response);
                }

                return documents;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return new List<ApprovalDetailModel>(); // No documents found
            }
            catch (Exception ex)
            {
                // Log the exception
                throw; // Re-throw the exception for external handling
            }
        }

        public async Task<List<Approved>> GetAllRejectedDocumentByUserEmail(string targetEmail)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE (CONTAINS(LOWER(c.Status), 're')) AND c.ApproverEmail = @targetEmail")
                    .WithParameter("@targetEmail", targetEmail);
                var document = new List<Approved>();
                var iterator = _approveContainer.GetItemQueryIterator<Approved>(query);

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    document.AddRange(response);
                }

                return document;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<Approved>(); // No documents found
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
        public async Task<List<CreateRequestDTOs>> GetDocumentsAndEmailAsync(string targetEmail, string documentName)
        {
            try
            {
                targetEmail = targetEmail.ToLower();
                var query = new QueryDefinition("SELECT * FROM c WHERE ARRAY_CONTAINS(c.Approvals, @targetEmail) AND c.id = @documentName")
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
        public async Task<List<EmailModel>> GetDocumentByEmail(string userEmail, string Id)
        {
            try
            {
                var query = new QueryDefinition("SELECT c.Approvals FROM c WHERE c.Email = @userEmail AND c.Id = @Id")
                    .WithParameter("@userEmail", userEmail)
                    .WithParameter("@Id", Id);

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
        public async Task<List<CreateRequestDTOs>> GetDocumentByNameAsync(string targetEmail, Guid Id)
        {
           try
            {
                targetEmail = targetEmail.ToLower();
                var query = new QueryDefinition("SELECT * FROM c WHERE ARRAY_CONTAINS(c.Approvals, @targetEmail) AND c.id = @id")
                            .WithParameter("@targetEmail", targetEmail)
                            .WithParameter("@id", Id);
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
        public async Task<List<CreateRequestDTOs>> GetAllRequestCreatedByUserMail(string targetEmail)
        {
            try
            {
                targetEmail = targetEmail.ToLower();
                var query = new QueryDefinition("SELECT * FROM c WHERE LOWER(c.Email) = @targetEmail")
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
        public async Task DeleteItemAsync(string id)
        {
            await _container.DeleteItemAsync<CreateRequest>(id, new PartitionKey(id));
        }
        public async Task<List<ApprovalDetailModel>> GetAlreadyApproveDocumentByEmail(string Id, string email)
        {
            try
            {
                var query = new QueryDefinition("SELECT * FROM c WHERE c.Id = @Id AND c.ApproverName = @email")
                   .WithParameter("@Id", Id)
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
        public async Task<List<CreateRequestDTOs>> UpdateApproveDocumentByEmail(string targetEmail, string Id)
        {
            try
            {
                targetEmail = targetEmail.ToLower();
                var query = new QueryDefinition("SELECT * FROM c WHERE ARRAY_CONTAINS(c.Approvals, @targetEmail) AND c.Id = @Id")
                            .WithParameter("@targetEmail", targetEmail)
                            .WithParameter("@Id", Id);
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

        public async Task<List<CreateRequestDTOs>> UpdateDocumentAsync(string targetEmail, Guid Id)
        {
            try
            {
                targetEmail = targetEmail.ToLower();

                // Assuming 'Id' is the partition key
                var partitionKey = new PartitionKey(Id.ToString());

                var query = new QueryDefinition("SELECT * FROM c WHERE c.Email = @targetEmail AND c.Id = @Id")
                                .WithParameter("@targetEmail", targetEmail)
                                .WithParameter("@Id", Id);
                var documentsToUpdate = new List<CreateRequestDTOs>();
                var iterator = _container.GetItemQueryIterator<CreateRequestDTOs>(query, requestOptions: new QueryRequestOptions { PartitionKey = partitionKey });

                while (iterator.HasMoreResults)
                {
                    var response = await iterator.ReadNextAsync();
                    documentsToUpdate.AddRange(response);
                }

                // Update each document
                foreach (var document in documentsToUpdate)
                {
                    // Update document properties as needed
                    document.ApprovalStatus = "Approved"; // Example update, modify according to your needs

                    // Replace the document in Cosmos DB
                    await _container.ReplaceItemAsync(document, document.Id.ToString(), partitionKey);
                }

                return documentsToUpdate;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return new List<CreateRequestDTOs>(); // No documents found
            }
        }

    }
}
