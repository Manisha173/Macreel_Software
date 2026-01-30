using Macreel_Software.Contracts.DTOs;
using Macreel_Software.DAL.Master;
using Macreel_Software.Models;
using Macreel_Software.Models.Master;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Macreel_Software.Server.Controllers
{
    [Authorize(Roles ="admin")]
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
                int result = await _service.InsertRole(data);

                if (result == 1)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(
                        null,
                        "Role inserted successfully"
                    ));
                }

                if (result == 2)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(
                        null,
                        "Role updated successfully"
                    ));
                }

                if (result == -1)
                {
                    return StatusCode(409, ApiResponse<object>.FailureResponse(
                        "Role already exists",
                        409
                    ));
                }

                return BadRequest(ApiResponse<object>.FailureResponse(
                    "Some error occurred while saving role",
                    400
                ));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(
                    "Internal server error",
                    500
                ));
            }
        }




        [HttpGet("getRoleById")]
        public async Task<IActionResult> getRoleById(int roleId)
        {
            try
            {
          
                var result = await _service.getAllRoleById(roleId);

           
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<role>.FailureResponse(
                    "An error occurred while fetching role.",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }

        [HttpDelete("deleteRoleById")]
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
                int result = await _service.insertDepartment(data);

                if (result == 1)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(
                        null,
                        "Department inserted successfully"
                    ));
                }

                if (result == 2)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(
                        null,
                        "Department updated successfully"
                    ));
                }

                if (result == -1)
                {
                    return StatusCode(409, ApiResponse<object>.FailureResponse(
                        "Department already exists",
                        409
                    ));
                }

                return BadRequest(ApiResponse<object>.FailureResponse(
                    "Some error occurred while saving department",
                    400
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(
                    "Internal server error",
                    500
                ));
            }
        }




        [HttpGet("getDepartmentById")]
        public async Task<IActionResult> getDepartmentById(int depId)
        {
            try
            {
                var response = await _service.getAllDepartmentById(depId);

                if (response.Success)
                {
                    return Ok(response);
                }
                else
                {
                    return StatusCode(response.StatusCode, response);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<department>>.FailureResponse(
                    "An unexpected error occurred while fetching Department.",
                    500,
                    errorCode: "EXCEPTION"
                ));
            }
        }


        [HttpDelete("deleteDepartmentById")]
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
                int result = await _service.InsertDesignation(data);

                if (result == 1)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(
                        null,
                        "Designation inserted successfully"
                    ));
                }

                if (result == 2)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(
                        null,
                        "Designation updated successfully"
                    ));
                }

                if (result == -1)
                {
                    return StatusCode(409, ApiResponse<object>.FailureResponse(
                        "Designation already exists",
                        409
                    ));
                }

                return BadRequest(ApiResponse<object>.FailureResponse(
                    "Some error occurred while saving designation",
                    400
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(
                    "Internal server error",
                    500
                ));
            }
        }


   

        [HttpGet("getDesignationById")]
        public async Task<IActionResult> getDesignationById(int desId)
        {
            try
            {
                var response = await _service.getAllDesignationById(desId);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<designation>>.FailureResponse(
                    "An error occurred while fetching designation.",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }


        [HttpDelete("deleteDesignationById")]
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

        #region technology api

        [HttpPost("insertTechnology")]
        public async Task<IActionResult> insertTechnology([FromBody]technology data)
        {
            try
            {
                string[] softwareTypes = new string[] { "app", "web" };
                if (!softwareTypes.Contains(data.SoftwareType))
                {
                    return Ok(ApiResponse<object>.FailureResponse(
                        "Software type should be only web/app",
                        400
                        ));
                }
                int result = await _service.insertTechnology(data);

                if (result == 1)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(
                        null,
                        "Technology inserted successfully"
                    ));
                }

                if (result == 2)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(
                        null,
                        "Technology updated successfully"
                    ));
                }

                if (result == -1)
                {
                    return StatusCode(409, ApiResponse<object>.FailureResponse(
                        "Technology already exists",
                        409
                    ));
                }

                return BadRequest(ApiResponse<object>.FailureResponse(
                    "Some error occurred while saving Technology",
                    400
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(
                    "Internal server error",
                    500
                ));
            }
        }

        [HttpDelete("deleteTechnologyById")]
        public async Task<IActionResult> DeleteTechnologyById(int id)
        {
            try
            {
                bool isDeleted = await _service.deleteTechnologyById(id);

                if (isDeleted)
                {
                    return Ok(new
                    {
                        status = true,
                        statusCode = 200,
                        message = "Technology deleted successfully."
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = false,
                        statusCode = 404,
                        message = "Technology not found or already deleted."
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    statusCode = 500,
                    message = "An error occurred while deleting technology.",
                    error = ex.Message
                });
            }
        }
        #endregion

        #region
        [HttpGet("EmpListForWebTypeByTechId")]
        public async Task<IActionResult> EmpListForWebTypeByTechId(int techId)
        {
            try
            {
                var result = await _service.EmpListForWebByTechId(techId);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ApiResponse<List<technologyDetails>>.FailureResponse("An error occured while fetching skills for web type!!", 500, errorCode: "Server_Error"));
            }
        }

        [HttpGet("EmpListForAppBtTechId")]
        public async Task<IActionResult> EmpListForAppBtTechId(int techId)
        {
            try
            {
                var result = await _service.empListForAppByTechId(techId);
                return Ok(result);
            }
            catch(Exception ex)
            {
                return StatusCode(500, ApiResponse<List<technologyDetails>>.FailureResponse("An error occured while fetching skills for App type!!", 500, errorCode: "Server_Error"));
            }
        }

        #endregion

        #region page api

        [HttpPost("insertPage")]
        public async Task<IActionResult> insertPage([FromBody] Page data)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.FailureResponse(
                        ModelState.Values
                            .SelectMany(v => v.Errors)
                            .FirstOrDefault()?.ErrorMessage!,
                        400
                    ));
                }
                bool result = await _service.InsertUpdatePage(data);

                if (data.id > 0 && result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(
                        null,
                        "Page updated successfully"
                    ));
                }

                if (data.id == 0 && result)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(
                        null,
                        "Page inserted successfully"
                    ));
                }

                return BadRequest(ApiResponse<object>.FailureResponse(
                    "Some error occurred while saving page",
                    400
                ));
            }
            catch (SqlException ex) when (ex.Number == 50000)
            {
                return StatusCode(409, ApiResponse<object>.FailureResponse(
                    ex.Message,
                    409
                ));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(
                    "Internal server error",
                    500
                ));
            }
        }


        [HttpGet("getAllPages")]
        public async Task<IActionResult> getAllPages(int? pageNumber = null,int? pageSize = null)
        {
            try
            {
                ApiResponse<List<Page>> result =
                    await _service.GetAllPages(null, pageNumber, pageSize);

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<List<Page>>.FailureResponse(
                    "An error occurred while fetching pages",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }

        [HttpGet("getPageById")]
        public async Task<IActionResult> getPageById(int pageId)
        {
            try
            {
                var result = await _service.GetAllPages(pageId, null, null);

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<Page>.FailureResponse(
                    "An error occurred while fetching page.",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }


        [HttpDelete("deletePageById")]
        public async Task<IActionResult> deletePageById(int pageId)
        {
            try
            {
                var res = await _service.DeletePageById(pageId);

                if (res)
                {
                    return Ok(new
                    {
                        status = true,
                        StatusCode = 200,
                        message = "Page deleted successfully!!!"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = false,
                        StatusCode = 404,
                        message = "Page not deleted!!"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    StatusCode = 500,
                    message = "An error occurred while deleting page.",
                    error = ex.Message
                });
            }
        }
        [HttpPost("assignRolePages")]
        public async Task<IActionResult> AssignRolePages([FromBody] AssignPage data)
        {
            try
            {
                // 🔹 Model validation
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.FailureResponse(
                        ModelState.Values
                            .SelectMany(v => v.Errors)
                            .FirstOrDefault()?.ErrorMessage!,
                        400
                    ));
                }

                if (data.pages == null || !data.pages.Any())
                {
                    return BadRequest(ApiResponse<object>.FailureResponse(
                        "At least one page is required",
                        400
                    ));
                }

                var result = await _service.AssignOrUpdateRolePages(data);

                if (result != null)
                {
                    return Ok(ApiResponse<object>.SuccessResponse(
                        null,
                        "Role pages assigned/updated successfully"
                    ));
                }

                return BadRequest(ApiResponse<object>.FailureResponse(
                    "Some error occurred while assigning pages",
                    400
                ));
            }
            catch (SqlException ex) when (ex.Number == 50000)
            {
                // 🔥 Custom SQL THROW error
                return StatusCode(409, ApiResponse<object>.FailureResponse(
                    ex.Message,
                    409
                ));
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(
                    "Internal server error",
                    500
                ));
            }
        }
        [HttpGet("get-pages-by-role/{roleId?}")]
        public async Task<IActionResult> GetPagesByRole(int? roleId)
        {
            try
            {
                var result = await _service.GetAllAssignedPages(roleId);

                return result.Success
                    ? Ok(result)
                    : BadRequest(result);
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<object>.FailureResponse(
                        "Internal server error",
                        500
                    ));
            }
        }
        [HttpGet("getAllAssignedPages")]
        public async Task<IActionResult> GetAllAssignedPages(string? searchTerm = null,int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                ApiResponse<List<RolePagesDto>> result =
                    await _service.GetAllAssignedPages(null, pageNumber, pageSize,searchTerm);

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception)
            {
                return StatusCode(500, ApiResponse<List<Page>>.FailureResponse(
                    "An error occurred while fetching pages",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }
        #endregion
    }
}
