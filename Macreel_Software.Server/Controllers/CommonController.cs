using System.Security.Claims;
using Macreel_Software.Contracts.DTOs;
using Macreel_Software.DAL.Admin;
using Macreel_Software.DAL.Common;
using Macreel_Software.DAL.Master;
using Macreel_Software.Models;
using Macreel_Software.Models.Common;
using Macreel_Software.Models.Master;
using Macreel_Software.Services.FileUpload.Services;
using Macreel_Software.Services.MailSender;
using Microsoft.AspNetCore.Mvc;

namespace Macreel_Software.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommonController : ControllerBase
    {
        private readonly IAdminServices _services;
        private readonly ICommonServices _service;
        private readonly PasswordEncrypt _pass;
        private readonly FileUploadService _fileUploadService;
        private readonly IAdminServices _adminservice;
        private readonly MailSender _mailservice;
        private readonly int _userId;
        private readonly IMasterService _masterservice;
        private readonly string _role;


        public CommonController(ICommonServices service, PasswordEncrypt pass, FileUploadService fileUploadService,IAdminServices adminservice, MailSender mailservice, IAdminServices adminService, IHttpContextAccessor http,IMasterService masterservice)
        {
            _service = service;
            _pass = pass;
            _fileUploadService = fileUploadService;
            _adminservice = adminservice;
            _mailservice = mailservice;
            _services = adminService;
            _masterservice = masterservice;
            var user = http.HttpContext?.User;
            if (user != null && user.Identity?.IsAuthenticated == true)
            {
                _userId = Convert.ToInt32(user.FindFirst("UserId")?.Value);
                _role = user.FindFirst(ClaimTypes.Role)?.Value.ToString()!;
            }
        }

        [HttpGet("getAllStateList")]
        public async Task<IActionResult> getAllState()
        {
            try
            {
                var data = await _service.GetAllState(); 

                if (data != null && data.Count > 0)
                {
                    return Ok(new
                    {
                        status = true,
                        StatusCode = 200,
                        message = "State list fetched successfully!!",
                        stateList = data
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
                    message = "An error occurred while fetching states.",
                    error = ex.Message
                });
            }
        }

        [HttpGet("GetCityByStateId")]
        public async Task<IActionResult> getCityByStateId(int stateId)
        {
            try
            {
                var data = await _service.getCityById(stateId);
                if (data != null && data.Any())
                {
                    return Ok(new
                    {
                        status=true,
                        StatusCode=200,
                        message="City List by state id fetch successfully!!",
                        CityList=data
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
            catch(Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    StatusCode = 500,
                    message = "An error occurred while fetching cities.",
                    error = ex.Message
                });
            }
        }

        #region reporting manager

        [HttpGet("getReportingManager")]
        public async Task<IActionResult> GetReportingManager()
        {
            try
            {
                var result = await _services.GetAllReportingManager();

                if (result != null && result.Any())
                {
                    return Ok(ApiResponse<List<ReportingManger>>.SuccessResponse(
                        result,
                        "Reporting manager fetched successfully"
                    ));
                }

                return Ok(ApiResponse<List<ReportingManger>>.FailureResponse(
                    "No data found",
                    404
                ));
            }
            catch (Exception)
            {
                return StatusCode(500,
                    ApiResponse<List<ReportingManger>>.FailureResponse(
                        "An error occurred while fetching reporting managers",
                        500,
                        "SERVER_ERROR"
                    ));
            }
        }
        #endregion

        #region designation

        #endregion

        #region Register Admin
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin([FromQuery] string Username,string Password)
        {
            try
            {
                Password = _pass.EncryptPassword(Password);
                bool result = await _service.RegisterAdmin(Username,Password);

                return Ok(new
                {
                    status = true,
                    message = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }
        #endregion

        #region RuleBook
        [HttpPost("Add-Update-RuleBook")]
        public async Task<IActionResult> AddUpdateRuleBook([FromForm] ruleBook data)
        {
            if (data == null)
                return BadRequest("Invalid request data.");

            if (data.id == 0 && data.rule_Book == null)
                return BadRequest("RuleBook file is required.");

            try
            {
                string[] docExt = { ".pdf", ".doc", ".docx" };

                // ---------- STEP 1: VALIDATE + GENERATE PATH (ONLY IF FILE PROVIDED) ----------
                if (data.rule_Book != null)
                {
                    data.rule_Book_Path = _fileUploadService.ValidateAndGeneratePath(
                        data.rule_Book,
                        "ImportantFiles",
                        docExt
                    );
                }

                // ---------- STEP 2: DB INSERT / UPDATE ----------
                bool result = await _service.AddUpdateRuleBook(data);

                if (!result)
                {
                    return StatusCode(500, new
                    {
                        status = false,
                        message = "Failed to save RuleBook."
                    });
                }

                // ---------- STEP 3: ACTUAL FILE UPLOAD ----------
                if (data.rule_Book != null)
                {
                    await _fileUploadService.UploadAsync(
                        data.rule_Book,
                        data.rule_Book_Path!
                    );
                }

                return Ok(new
                {
                    status = true,
                    message = data.id > 0
                        ? "RuleBook updated successfully."
                        : "RuleBook added successfully."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    message = "Internal server error."
                });
            }
        }

        [HttpGet("getAllRuleBook")]
        public async Task<IActionResult> getAllRulebook(string? searchTerm = null, int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                ApiResponse<List<ruleBook>> result =
                    await _service.getAllRulrBook(searchTerm, pageNumber, pageSize);


                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ruleBook>>.FailureResponse(
                    "An error occurred while fetching rulebook",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }

        [HttpGet("getAllRuleBookById")]
        public async Task<IActionResult> getAllRulebookByID(int id)
        {
            try
            {
                ApiResponse<List<ruleBook>> result =
                    await _service.GetRuleBookByIdAsync(id);


                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<ruleBook>>.FailureResponse(
                    "An error occurred while fetching rulebook",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }


        [HttpDelete("deleteRuleBookById")]
        public async Task<IActionResult> deleteRuleBookById(int id)
        {
            try
            {
                var res = await _service.deleteRuleBookById(id);
                if (res)
                {
                    return Ok(new
                    {
                        status = true,
                        StatusCode = 200,
                        message = "RuleBook deleted successfully!!!"

                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = false,
                        StatusCode = 404,
                        message = "RuleBook not deleted!!"
                    });
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    status = false,
                    StatusCode = 500,
                    message = "An error occurred while deleting RuleBook.",
                    error = ex.Message
                });
            }
        }

        #endregion

        #region Leave
        [HttpGet("getAllLeave")]
        public async Task<IActionResult> getAllLeave(string? searchTerm = null, int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                ApiResponse<List<Leave>> result =
                    await _adminservice.getAllLeave(searchTerm, pageNumber, pageSize);


                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<role>>.FailureResponse(
                    "An error occurred while fetching leave",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }
        #endregion

        #region send mail for reg

        [HttpPost("sendEmailForReg")]
        public async Task<IActionResult> SendEmailForReg([FromBody] sendMailForReg data)
        {
            try
            {
                string accessId = Guid.NewGuid().ToString();
               

                var mailRequest = new MailRequest
                {
                    ToEmail = data.email,
                    Subject = "Register Yourself - Macreel Infosoft Pvt. Ltd.",
                    BodyType = MailBodyType.RegistrationLink,
                    Value = accessId
                };
                var mailResponse = await _mailservice.SendMailAsync(mailRequest);

                if (!mailResponse.Status)
                {
                    return BadRequest(new
                    {
                        status = false,
                        message = "Mail sending failed"
                    });
                }

                bool isSaved = await _service.sendMailForReg(data , accessId);

                if (!isSaved)
                {
                    return StatusCode(500, new
                    {
                        status = false,
                        message = "Mail sent but failed to save data"
                    });
                }

                return Ok(new
                {
                    status = true,
                    message = "Registration mail sent successfully"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    message = ex.Message
                });
            }
        }

        [HttpGet("EmailIdByAccessId")]
        public async Task<IActionResult> EmailIdByAccessId(string accessId)
        {
            try
            {

                var result = await _service.getEmailByAccessByIdForReg(accessId);


                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<SendEmailForRegistrationDto>.FailureResponse(
                    "An error occurred while fetching email.",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }



        #endregion

        #region Download File
        [HttpGet("download-file")]
        public IActionResult DownloadFile([FromQuery] string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                return BadRequest(new
                {
                    status = false,
                    message = "File path is required."
                });
            }

            try
            {
                // wwwroot base path
                var rootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                // Normalize & combine (prevents ../ attack)
                var fullPath = Path.GetFullPath(Path.Combine(rootPath, filePath));

                // Security check: ensure file is inside wwwroot
                if (!fullPath.StartsWith(rootPath))
                {
                    return BadRequest(new
                    {
                        status = false,
                        message = "Invalid file path."
                    });
                }

                if (!System.IO.File.Exists(fullPath))
                {
                    return NotFound(new
                    {
                        status = false,
                        message = "File not found."
                    });
                }

                var fileName = Path.GetFileName(fullPath);

                // Detect content type
                var provider = new Microsoft.AspNetCore.StaticFiles.FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(fullPath, out var contentType))
                {
                    contentType = "application/octet-stream";
                }

                var fileBytes = System.IO.File.ReadAllBytes(fullPath);

                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    message = "File download failed."
                });
            }
        }
        #endregion


        #region Assigned Project for employee

        [HttpPost("AssignProjectToEmp")]
        public async Task<IActionResult> AssignProjectToEmp([FromBody] ProjectEmp data)
        {
            try
            {
                bool res = await _service.InsertProjectEmp(data, _userId);

                if (res)
                {
                    return Ok(new
                    {
                        status = true,
                        statusCode = 200,
                        message = "Project assigned successfully"
                    });
                }
                else
                {
                    return BadRequest(new
                    {
                        status = false,
                        statusCode = 400,
                        message = "No record inserted"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    statusCode = 500,
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }

        [HttpGet("getAllProject")]
        public async Task<IActionResult> getAllProject(string? searchTerm = null, int? pageNumber = null, int? pageSize = null, string? status = null)
        {
            try
            {
                ApiResponse<List<project>> result =
                    await _services.GetAllProject(searchTerm, pageNumber, pageSize, _role == "admin"?null: _userId, status);


                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<project>>.FailureResponse(
                    "An error occurred while fetching project details",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }

        [HttpGet("AssignedProjectEmpList")]
        public async Task<IActionResult> AssignedProjectEmpList(int projectId)
        {
            try
            {
                ApiResponse<List<AssignedProjectEmpDto>> result =
                    await _service.AssignedProjectEmpList(projectId);


                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AssignedProjectEmpDto>>.FailureResponse(
                    "An error occurred while fetching assigned project emp list",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }

        [HttpPost("update-project-emp-status")]
        public async Task<IActionResult> UpdateProjectEmpStatus([FromBody] List<ProjectEmpStatusItem> model)
        {
            try
            {
                int adminId = _userId;

                foreach (var item in model)
                {
                    await _service.UpdateProjectEmpStatusSingle(item, adminId);
                }

                return Ok(new
                {
                    success = true,
                    message = "Project employee status updated successfully."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Some error occurred."
                });
            }
        }


        #endregion

        #region get all emp

        [HttpGet("GetAllEmployees")]
        public async Task<IActionResult> GetAllEmployees(string? searchTerm, int? pageNumber, int? pageSize)
        {
            try
            {
                string? role = _role;
                ApiResponse<List<employeeRegistration>> result =
                    await _services.GetAllEmpData(searchTerm, pageNumber, pageSize,_role);

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500,
                    ApiResponse<List<employeeRegistration>>.FailureResponse(
                        "An error occurred while fetching employee data",
                        500,
                        "SERVER_ERROR"));
            }
        }

        #endregion

        #region Role Assign Page
        [HttpGet("getAllAssignedPages")]
        public async Task<IActionResult> GetAllAssignedPages(string? searchTerm = null, int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                ApiResponse<List<RolePagesDto>> result =
                    await _masterservice.GetAllAssignedPages(null, pageNumber, pageSize, searchTerm);

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
