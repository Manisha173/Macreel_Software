using Macreel_Software.Contracts.DTOs;
using Macreel_Software.Models;
using Macreel_Software.Models.Employee;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macreel_Software.DAL.Employee
{
    public interface IEmployeeService
    {
        Task<bool> insertResponseByEmpId(EmployeeData data);
        Task<ApiResponse<List<assignedLeave>>> AssignedLeaveListByEmpId( int? empId, string? searchTerm, int? pageNumber,int? pageSize);
        Task<bool> insertApplyLeaveByEmpId(applyLeave data);
        Task<ApiResponse<List<applyLeave>>> applyLeaveListByEmpId(int empId, string? searchTerm,int? pageNumber, int? pageSize);
        Task<ApiResponse<List<applyLeave>>> getAllApplyLeaveById(int id, int empId);
        Task<bool> deleteApplyLeaveById(int id, int empId);
        Task<ApiResponse<Dashboard>> DashboardCount(int empId);
        Task<ApiResponse<List<AssignedLeaveDto>>> GetAllAssignedLeaveByEmpCode(int empcode);
        Task<ApiResponse<List<AssignedProjectDto>>> assignedProjectByEmpId(int empId, string? searchTerm, int? pageNumber, int? pageSize);
        Task<ApiResponse<List<AssignedTaskDto>>> assignedTaskData(int projectId, int empId);
        Task<bool> updateTaskStatus(UpdateTaskStatus data, int empId, string? document1Path, string? document2Path);
    }
}
