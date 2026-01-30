using Macreel_Software.Contracts.DTOs;
using Macreel_Software.Models;
using Macreel_Software.Models.Common;
using Microsoft.AspNetCore.Http;

namespace Macreel_Software.DAL.Common
{
    public interface ICommonServices
    {
        Task<List<state>> GetAllState();

        Task<List<city>> getCityById(int stateId);
        Task<bool> RegisterAdmin(string Username, string Password);
        Task<bool> AddUpdateRuleBook(ruleBook data);
        Task<ApiResponse<List<ruleBook>>> getAllRulrBook(string? searchTerm, int? pageNumber, int? pageSize);
        Task<ApiResponse<List<ruleBook>>> GetRuleBookByIdAsync(int id);
        Task<bool> deleteRuleBookById(int id);
        Task<bool> sendMailForReg(sendMailForReg data, string accessId);
        Task<ApiResponse<List<SendEmailForRegistrationDto>>> getEmailByAccessByIdForReg(string accessId);
        Task<bool> InsertProjectEmp(ProjectEmp data, int? userid,string? addedBy);
        Task<ApiResponse<List<AssignedProjectEmpDto>>> AssignedProjectEmpList(int projectId);
        Task<bool> UpdateProjectEmpStatusSingle(ProjectEmpStatusItem model, int adminId);
    }
}
