using DocumentApproval_Api.Entities;
using DocumentApproval_Api.RequestDTOs;
using DocumentApproval_DataSource.Entities;
using DocumentApproval_DTOs.RequestDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentApproval_Api.Interfaces
{
    public interface ICosmosDbService
    {
        Task<CreateRequest> CreateRequest(CreateRequest request);
        Task<ApprovalRequest> AddApprovalItemAsync(ApprovalRequest request);
        Task<CreateRequest> GetAll(CreateRequest request);
        Task<List<CreateRequestDTOs>> GetDocumentByNameAsync(string targetEmail, Guid Id);
        Task<ApprovalRequest> ApproveRequest(ApprovalRequest ApproveRequest);
        Task<List<CreateRequestDTOs>> GetApprovalEmailByUser(string targetEmail);
        Task<List<Approved>> GetAllApprovedDocumentByUserEmail(string targetEmail);
        Task<List<Approved>> GetAllRejectedDocumentByUserEmail(string targetEmail);
        Task<List<CreateRequestDTOs>> GetDocumentsAndEmailAsync(string targetEmail, string Id);
        Task<List<EmailModel>> GetDocumentByEmail(string userEmail, string Id);
        Task<List<ApprovalDetailModel>> GetAllApproveDocumentByEmail(string targetEmail, Guid Id);
        Task<List<ApprovalDetailModel>> GetAlreadyApproveDocumentByEmail(string Id, string email);
        Task<List<CreateRequestDTOs>>UpdateApproveDocumentByEmail(string targetEmail, string Id);
        Task<List<CreateRequestDTOs>> GetAllRequestCreatedByUserMail(string targetEmail);
        Task <List<CreateRequestDTOs>>UpdateDocumentAsync(string targetEmail, Guid Id);



    }
}
