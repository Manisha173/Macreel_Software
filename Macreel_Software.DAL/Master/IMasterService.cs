using Macreel_Software.Contracts.DTOs;
using Macreel_Software.Models;
using Macreel_Software.Models.Master;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macreel_Software.DAL.Master
{
    public interface IMasterService
    {
        Task<int> InsertRole(role data);
        Task<ApiResponse<List<role>>> getAllRole(string? searchTerm, int? pageNumber, int? pageSize);
        Task<ApiResponse<List<role>>> getAllRoleById(int id);

        Task<bool> deleteRoleById(int id);
        Task<int> insertDepartment(department data);
        Task<ApiResponse<List<department>>> getAllDepartment(string? searchTerm,int? pageNumber,int? pageSize);

        Task<ApiResponse<List<department>>> getAllDepartmentById(int id);
        Task<bool> deleteDepartmentById(int id);

        Task<int> InsertDesignation(designation data);

        Task<ApiResponse<List<designation>>> getAllDesignation(string? searchTerm, int? pageNumber, int? pageSize);


        Task<ApiResponse<List<designation>>> getAllDesignationById(int id);

        Task<bool> deleteDesignationById(int id);
        Task<int> insertTechnology(technology data);
      Task<ApiResponse<List<technology>>> getAllTechnology(string? searchTerm, int? pageNumber,int? pageSize);
        Task<ApiResponse<List<technology>>> getAllTechnologyById(int id);
        Task<bool> deleteTechnologyById(int id);
        Task<ApiResponse<List<technologyDetails>>> EmpListForWebByTechId(int id);
        Task<ApiResponse<List<technologyDetails>>> empListForAppByTechId(int id);
        Task<bool> DeletePageById(int id);
        Task<ApiResponse<List<Page>>> GetAllPages(int? id, int? pageNumber, int? pageSize);
        Task<bool> InsertUpdatePage(Page data);
        Task<ApiResponse<bool>> AssignOrUpdateRolePages(AssignPage data);
        Task<ApiResponse<List<RolePagesDto>>> GetAllAssignedPages(int? id = null, int? pageNumber = null, int? pageSize = null,string? searchTerm = null);
    }
}
