using DocumentApproval_DataSource.Entities;
using DocumentApproval_DTOs.RequestDTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentApproval_DataSource.Repository.Interface
{
    public interface ICosmosDbService
    {
        Task<CreateRequest> CreateRequest(CreateRequest request); 
        Task<CreateRequest> GetAll(CreateRequest request);
        Task<List<CreateRequestDTOs>> GetDocumentByNameAsync(string targetEmail, string documentName);
        Task<ApprovalRequest> ApproveRequest(ApprovalRequest ApproveRequest);
        Task<List<CreateRequestDTOs>> GetApprovalEmailByUser(string targetEmail);
        Task<List<ApprovalRequestDTOs>> GetAllApprovedDocumentByUserEmail(string targetEmail);
        Task<List<ApprovalRequestDTOs>> GetAllRejectedDocumentByUserEmail(string targetEmail);
        Task<List<CreateRequestDTOs>> GetDocumentByDocumentNameandEmail(string targetEmail, string documentName);
        Task<List<EmailModel>> GetDocumentByEmail(string userEmail, string documentName);
        Task<List<ApprovalDetailModel>> GetAllApproveDocumentByEmail(string documentName);
        Task<List<ApprovalDetailModel>> GetAlreadyApproveDocumentByEmail(string documentName, string email);

    }
}
