using Macreel_Software.Contracts.DTOs;
using Macreel_Software.Models;
using Macreel_Software.Models.Employee;
using Macreel_Software.Models.Master;

using Microsoft.AspNetCore.Http;

namespace Macreel_Software.DAL.Admin
{
    public interface IAdminServices
    {
        Task<string> InsertEmployeeRegistrationData(employeeRegistration data);
        Task<List<ReportingManger>> GetAllReportingManager();
         Task<ApiResponse<List<employeeRegistration>>> GetAllEmpData(string? searchTerm, int? pageNumber, int? pageSize, string? addedBy);
        Task<bool> deleteEmployeeById(int id);
        Task<ApiResponse<List<employeeRegistration>>> GetAllEmpDataById(int id);
        Task<bool> UpdateEmployeeRegistrationData(employeeRegistration data);

        Task<int> InsertLeave(Leave data);
        Task<ApiResponse<List<Leave>>> getAllLeave(string? searchTerm, int? pageNumber, int? pageSize);
        Task<ApiResponse<List<Leave>>> getAllLeaveById(int id);
        Task<bool> deleteLeaveById(int id);
        Task<bool> InsertAssignLeaveAsync(AssignLeave model);
        Task<ApiResponse<List<showLeave>>> getAllAssignedLeaveById(int empId);
        Task<int> UploadAttendance(IFormFile file, int selectedMonth, int currentYear);
        Task<ApiResponse<List<Attendance>>> EmpAttendanceDataByEmpCode(string empCode, int month, int year);

        Task<ApiResponse<List<EmpWorkingDetails>>> EmpWorkingDetailsByempCode(int empCode, int month, int year);

        Task<bool> AddProject(project data);
        Task<ApiResponse<List<project>>> GetAllProject(string? SearchTerm, int? pageNumber, int? pageSize, int? userId = null,string? status = null);
        Task<ApiResponse<List<project>>> GetAllProjectById(int id);
        Task<bool> deleteProjectById(int id);

        Task<ApiResponse<List<allAssignedLeave>>> getAllAssignedLeave(string? searchTerm, int? pageNumber, int? pageSize);
        Task<ApiResponse<List<project>>> GetEmpProjectDetailByEmpId(int empId);

        Task<bool> insertTask(Taskassign data);
        Task<ApiResponse<List<TaskAssignDto>>> getAllAssignTask(string? searchTerm, int? pageNumber, int? pageSize, int? empId = null);

        Task<ApiResponse<List<Taskassign>>> getAllAssignTaskById(int id);
        Task<bool> deleteTaskById(int id);
        Task<ApiResponse<List<applyLeave>>> GetAllLeaveRequests(string? searchTerm, int? pageNumber, int? pageSize);
        Task<bool> UpdateLeaveRequest(int id, int status, string? reason = null);

        Task<ApiResponse<List<AdminDashboardCountDto>>> adminDashboardCount();
        Task<ApiResponse<List<AssignedProjectEmpListDto>>> assignedProjectEmpList(int projectId, int pmId);
    }
}
