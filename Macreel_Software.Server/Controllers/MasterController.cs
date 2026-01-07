using Macreel_Software.DAL.Common;
using Macreel_Software.DAL.Master;
using Macreel_Software.Models.Master;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Macreel_Software.Server;
using Macreel_Software.Models;

namespace Macreel_Software.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MasterController : ControllerBase
    {
        private readonly IMasterService _service;

        public MasterController(IMasterService service)
        {
            _service = service;
        }

        #region role api
        [HttpPost("insertRole")]
        public async Task<IActionResult> addrole([FromBody] role data)
        {
            try
            {
                bool res = await _service.InsertRole(data);

                if (res)
                {
                    return Ok(new
                    {
                        status = true,
                        StatusCode = 200,
                        message = data.id>0?"Role updated successfully!!":"Role inserted successfully"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        status = false,
                        StatusCode = 400,
                        message = data.id > 0 ? "some error occured during updation!!" : "some error occured during insertion"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    StatusCode = 500,
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }

        [HttpGet("getAllRole")]
        public async Task<IActionResult> getAllRole(string? searchTerm = null, int? pageNumber = null,int? pageSize = null)
        {
            try
            {
                var result = await _service.getAllRole(searchTerm, pageNumber, pageSize);

                if (result.Data != null && result.Data.Any())
                {
                 
                    if (pageNumber.HasValue && pageSize.HasValue)
                    {
                        return Ok(ApiResponse<List<role>>.PagedResponse(
                            result.Data,
                            pageNumber.Value,
                            pageSize.Value,
                            result.TotalRecords,
                            "Role data fetched successfully"
                        ));
                    }

                   
                    return Ok(ApiResponse<List<role>>.SuccessResponse(
                        result.Data,
                        "Role data fetched successfully"
                    ));
                }

                return Ok(ApiResponse<List<role>>.FailureResponse(
                    "No data found",
                    404
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<role>>.FailureResponse(
                    "An error occurred while fetching role",
                    500,
                    errorCode: "SERVER_ERROR"
                ));
            }
        }


        [HttpGet("getRoleById")]
        public async Task<IActionResult> getRoleById(int roleId)
        {
            try
            {
                var data = await _service.getAllRoleById(roleId);
                if (data != null && data.Any())
                {
                    return Ok(new
                    {
                        status = true,
                        StatusCode = 200,
                        message = "Role data get by id successfully!!!",
                        RoleListbyid = data
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = false,
                        StatusCode = 404,
                        message = "No data found!!"
                    });
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    status = false,
                    StatusCode = 500,
                    message = "An error occurred while fetching role.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("deleteRoleById")]
        public async Task<IActionResult> deleteRoleById(int roleId)
        {
            try
            {
                var res = await _service.deleteRoleById(roleId);
                if (res)
                {
                    return Ok(new
                    {
                        status = true,
                        StatusCode = 200,
                        message = "Role deleted successfully!!!"
                     
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = false,
                        StatusCode = 404,
                        message = "Role not deleted!!"
                    });
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    status = false,
                    StatusCode = 500,
                    message = "An error occurred while deleting role.",
                    error = ex.Message
                });
            }
        }

        #endregion

        #region department api

        [HttpPost("insertDepartment")]
        public async Task<IActionResult> addDepartment([FromBody] department data)
        {
            try
            {
                bool res = await _service.insertDepartment(data);

                if (res)
                {
                    return Ok(new
                    {
                        status = true,
                        StatusCode = 200,
                        message = data.id > 0 ? "Department updated successfully!!" : "Department inserted successfully"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        status = false,
                        StatusCode = 400,
                        message = data.id > 0 ? "some error occured during updation!!" : "some error occured during insertion"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    StatusCode = 500,
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }


        [HttpGet("getAllDepartment")]
        public async Task<IActionResult> getAllDepartment(string? searchTerm = null,int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                var result = await _service.getAllDepartment(searchTerm, pageNumber, pageSize);

                if (result.Data != null && result.Data.Any())
                {
                  
                    if (pageNumber.HasValue && pageSize.HasValue)
                    {
                        return Ok(ApiResponse<List<department>>.PagedResponse(
                            result.Data,
                            pageNumber.Value,
                            pageSize.Value,
                            result.TotalRecords,
                            "Department data fetched successfully"
                        ));
                    }

                 
                    return Ok(ApiResponse<List<department>>.SuccessResponse(
                        result.Data,
                        "Department data fetched successfully"
                    ));
                }

                return Ok(ApiResponse<List<department>>.FailureResponse(
                    "No data found",
                    404
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<department>>.FailureResponse(
                    "An error occurred while fetching department",
                    500,
                    errorCode: "SERVER_ERROR"
                ));
            }
        }


        [HttpGet("getDepartmentById")]
        public async Task<IActionResult> getDepartmentById(int depId)
        {
            try
            {
                var data = await _service.getAllDepartmentById(depId);
                if (data != null && data.Any())
                {
                    return Ok(new
                    {
                        status = true,
                        StatusCode = 200,
                        message = "Department data get by id successfully!!!",
                        DepartmentListbyid = data
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = false,
                        StatusCode = 404,
                        message = "No data found!!"
                    });
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    status = false,
                    StatusCode = 500,
                    message = "An error occurred while fetching Department.",
                    error = ex.Message
                });
            }
        }


        [HttpGet("deleteDepartmentById")]
        public async Task<IActionResult> deleteDepartmetById(int depId)
        {
            try
            {
                var res = await _service.deleteDepartmentById(depId);
                if (res)
                {
                    return Ok(new
                    {
                        status = true,
                        StatusCode = 200,
                        message = "Department deleted successfully!!!"

                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = false,
                        StatusCode = 404,
                        message = "Department not deleted!!"
                    });
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    status = false,
                    StatusCode = 500,
                    message = "An error occurred while deleting Department.",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region designation

        [HttpPost("insertDesignation")]
        public async Task<IActionResult> AddDesignation([FromBody] designation data)
        {
            try
            {
                bool res = await _service.InsertDesignation(data);

                if (res)
                {
                    return Ok(new
                    {
                        status = true,
                        StatusCode = 200,
                        message = data.id > 0 ? "Designation updated successfully!!" : "Designation inserted successfully"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        status = false,
                        StatusCode = 400,
                        message = data.id > 0 ? "some error occured during updation!!" : "some error occured during insertion"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    StatusCode = 500,
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }



        [HttpGet("getAllDesignation")]
        public async Task<IActionResult> getAllDesignation(string? searchTerm = null,int? pageNumber = null,
       int? pageSize = null)
        {
            try
            {
                var result = await _service.getAllDesignation(searchTerm, pageNumber, pageSize);

                if (result.Data != null && result.Data.Any())
                {
                  
                    if (pageNumber.HasValue && pageSize.HasValue)
                    {
                        return Ok(ApiResponse<List<designation>>.PagedResponse(
                            result.Data,
                            pageNumber.Value,
                            pageSize.Value,
                            result.TotalRecords,
                            "Designation data fetched successfully"
                        ));
                    }

                 
                    return Ok(ApiResponse<List<designation>>.SuccessResponse(
                        result.Data,
                        "Designation data fetched successfully"
                    ));
                }

                return Ok(ApiResponse<List<designation>>.FailureResponse(
                    "No data found",
                    404
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<designation>>.FailureResponse(
                    "An error occurred while fetching designation",
                    500,
                    errorCode: "SERVER_ERROR"
                ));
            }
        }


        [HttpGet("getDesignationById")]
        public async Task<IActionResult> getDesignationById(int desId)
        {
            try
            {
                var data = await _service.getAllDesignationById(desId);
                if (data != null && data.Any())
                {
                    return Ok(new
                    {
                        status = true,
                        StatusCode = 200,
                        message = "Designation data get by id successfully!!!",
                        DesignationListbyid = data
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = false,
                        StatusCode = 404,
                        message = "No data found!!"
                    });
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    status = false,
                    StatusCode = 500,
                    message = "An error occurred while fetching Designation.",
                    error = ex.Message
                });
            }
        }


        [HttpGet("deleteDesignationById")]
        public async Task<IActionResult> deleteDesignationById(int desId)
        {
            try
            {
                var res = await _service.deleteDesignationById(desId);
                if (res)
                {
                    return Ok(new
                    {
                        status = true,
                        StatusCode = 200,
                        message = "Designation deleted successfully!!!"

                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = false,
                        StatusCode = 404,
                        message = "Designation not deleted!!"
                    });
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    status = false,
                    StatusCode = 500,
                    message = "An error occurred while deleting Designation.",
                    error = ex.Message
                });
            }
        }
        #endregion

    }
}
