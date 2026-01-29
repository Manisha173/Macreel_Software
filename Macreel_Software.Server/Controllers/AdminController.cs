using System.Security.Claims;
using Macreel_Software.Contracts.DTOs;
using Macreel_Software.DAL.Admin;
using Macreel_Software.Models;
using Macreel_Software.Models.Common;
using Macreel_Software.Models.Employee;
using Macreel_Software.Models.Master;
using Macreel_Software.Services.FileUpload.Services;
using Macreel_Software.Services.MailSender;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Macreel_Software.Server.Controllers
{
    [Authorize(Roles = "admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminServices _services;
        private readonly FileUploadService _fileUploadService;
        private readonly IWebHostEnvironment _env;
        private readonly MailSender _mailservice;
        private readonly PasswordEncrypt _pass;
        private readonly int _userId;
        private readonly string _role;

        public AdminController(
            IAdminServices service,
            FileUploadService fileUploadService,
            IWebHostEnvironment env,
            MailSender mailservice,
            PasswordEncrypt pass, IHttpContextAccessor http)
        {
            _services = service;
            _fileUploadService = fileUploadService;
            _env = env;
            _mailservice = mailservice;
            _pass = pass;
            var user = http.HttpContext?.User;
            if (user != null && user.Identity?.IsAuthenticated == true)
            {
                _userId = Convert.ToInt32(user.FindFirst("UserId")?.Value);
                _role = user.FindFirst(ClaimTypes.Role)?.Value.ToString()!;
            }

        }

        [HttpGet("checkauth")]
        public IActionResult CheckAuth()
        {
            return Ok(new
            {
                StatusCode = 200,
                message = "ho gya"
            });
        }


        #region Manage Employee        
        [HttpPost("insertEmployeeRegistration")]
        public async Task<IActionResult> InsertEmployee([FromForm] employeeRegistration model)
        {
            try
            {
                model.addedBy = _role;
                string[] imgExt = { ".jpg", ".jpeg", ".png" };
                string[] docExt = { ".pdf", ".jpg", ".jpeg", ".png" };
                long maxSize = 2 * 1024 * 1024; // 2 MB

                var files = new[]
                {
                    new { File = model.ProfilePic, Path = (Action<string>)(p => model.ProfilePicPath = p), Ext = imgExt, Name = "Profile Picture" },
                    new { File = model.AadharImg, Path = (Action<string>)(p => model.AadharImgPath = p), Ext = imgExt, Name = "Aadhar Front Image" },
                    new { File = model.AadharBackImg, Path = (Action<string>)(p => model.AadharBackImgPath = p), Ext = imgExt, Name = "Aadhar Back Image" },
                    new { File = model.PanImg, Path = (Action<string>)(p => model.PanImgPath = p), Ext = imgExt, Name = "PAN Front Image" },
                    new { File = model.PanBackImg, Path = (Action<string>)(p => model.PanBackImgPath = p), Ext = imgExt, Name = "PAN Back Image" },
                    new { File = model.ExperienceCertificate, Path = (Action<string>)(p => model.ExperienceCertificatePath = p), Ext = docExt, Name = "Experience Certificate" },
                    new { File = model.TenthCertificate, Path = (Action<string>)(p => model.TenthCertificatePath = p), Ext = docExt, Name = "10th Certificate" },
                    new { File = model.TwelthCertificate, Path = (Action<string>)(p => model.TwelthCertificatePath = p), Ext = docExt, Name = "12th Certificate" },
                    new { File = model.GraduationCertificate, Path = (Action<string>)(p => model.GraduationCertificatePath = p), Ext = docExt, Name = "Graduation Certificate" },
                    new { File = model.MastersCertificate, Path = (Action<string>)(p => model.MastersCertificatePath = p), Ext = docExt, Name = "Masters Certificate" },
                };

                // ---------- STEP 1: FILE VALIDATION ----------
                foreach (var f in files)
                {
                    if (f.File == null) continue;

                    if (f.File.Length > maxSize)
                    {
                        return BadRequest(new
                        {
                            status = false,
                            statusCode = 400,
                            message = $"{f.Name} size should not exceed 2 MB"
                        });
                    }

                    try
                    {
                        f.Path(_fileUploadService.ValidateAndGeneratePath(
                            f.File,
                            "EmployeeFiles",
                            f.Ext
                        ));
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new
                        {
                            status = false,
                            statusCode = 400,
                            message = $"{f.Name} error: {ex.Message}"
                        });
                    }
                }

                string pass = model.Password;

                // ---------- STEP 2: DB INSERT ----------
                model.Password = _pass.EncryptPassword(model.Password!);
                string dbMessage = await _services.InsertEmployeeRegistrationData(model);



                if (dbMessage.Contains("Email already exists"))
                {
                    return Conflict(new
                    {
                        status = false,
                        statusCode = 409,
                        message = "Email already registered"
                    });
                }

                try
                {
                    MailRequest mailRequest = new MailRequest
                    {
                        ToEmail = model.EmailId,
                        Subject = "Your Login Credentials - Macreel Infosoft",
                        BodyType = MailBodyType.UserCredential,
                        UserName = model.EmailId,
                        Password = pass
                    };

                    await _mailservice.SendMailAsync(mailRequest);
                }
                catch(Exception ex)
                {
                    throw;
                }
                if (!dbMessage.ToLower().Contains("success"))
                {
                    return BadRequest(new
                    {
                        status = false,
                        statusCode = 400,
                        message = dbMessage
                    });
                }

                // ---------- STEP 3: FILE UPLOAD ----------
                foreach (var f in files)
                {
                    if (f.File != null)
                        await _fileUploadService.UploadAsync(f.File,
                                f.File == model.ProfilePic ? model.ProfilePicPath!
                                : f.File == model.AadharImg ? model.AadharImgPath!
                                : f.File == model.AadharBackImg ? model.AadharBackImgPath!
                                : f.File == model.PanImg ? model.PanImgPath!
                                : f.File == model.PanBackImg ? model.PanBackImgPath!
                                : f.File == model.ExperienceCertificate ? model.ExperienceCertificatePath!
                                : f.File == model.TenthCertificate ? model.TenthCertificatePath!
                                : f.File == model.TwelthCertificate ? model.TwelthCertificatePath!
                                : f.File == model.GraduationCertificate ? model.GraduationCertificatePath!
                                : model.MastersCertificatePath!
                        );
                }
                return Ok(new
                {
                    status = true,
                    statusCode = 201,
                    message = "Employee registered successfully"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    status = false,
                    statusCode = 500,
                    message = "Internal server error. Please try again later."
                });
            }
        }


        [HttpDelete("deleteEmployeeById")]
        public async Task<IActionResult> deleteEmployeeById(int id)
        {
            var res = await _services.deleteEmployeeById(id);

            if (res)
            {
                return Ok(new
                {
                    status = true,
                    StatusCode = 200,
                    message = "Employee deleted successfully!!!"
                });
            }

            return NotFound(new
            {
                status = false,
                StatusCode = 404,
                message = "Employee not found or already deleted!"
            });
        }

        [HttpGet("getEmployeeById")]
        public async Task<IActionResult> getemployeeById(int id)
        {
            try

            { 

                var result = await _services.GetAllEmpDataById(id);

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<role>.FailureResponse(
                    "An error occurred while fetching employee.",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }

        [HttpPost("updateEmployeeRegistration")]
        public async Task<IActionResult> UpdateEmployee([FromForm] employeeRegistration model)
        {
            try
            {
                string[] imgExt = { ".jpg", ".jpeg", ".png" };
                string[] docExt = { ".pdf", ".jpg", ".jpeg", ".png" };
                long maxSize = 2 * 1024 * 1024; // 2 MB

                var files = new[]
                {
            new { File = model.ProfilePic, Name = "Profile Picture", Ext = imgExt,
                Set = (Action<string>)(p => model.ProfilePicPath = p),
                Get = (Func<string?>)(() => model.ProfilePicPath) },

            new { File = model.AadharImg, Name = "Aadhar Front Image", Ext = imgExt,
                Set = (Action<string>)(p => model.AadharImgPath = p),
                Get = (Func<string?>)(() => model.AadharImgPath) },

            new { File = model.AadharBackImg, Name = "Aadhar Back Image", Ext = imgExt,
                Set = (Action<string>)(p => model.AadharBackImgPath = p),
                Get = (Func<string?>)(() => model.AadharBackImgPath) },

            new { File = model.PanImg, Name = "PAN Front Image", Ext = imgExt,
                Set = (Action<string>)(p => model.PanImgPath = p),
                Get = (Func<string?>)(() => model.PanImgPath) },

            new { File = model.PanBackImg, Name = "PAN Back Image", Ext = imgExt,
                Set = (Action<string>)(p => model.PanBackImgPath = p),
                Get = (Func<string?>)(() => model.PanBackImgPath) },

            new { File = model.ExperienceCertificate, Name = "Experience Certificate", Ext = docExt,
                Set = (Action<string>)(p => model.ExperienceCertificatePath = p),
                Get = (Func<string?>)(() => model.ExperienceCertificatePath) },

            new { File = model.TenthCertificate, Name = "10th Certificate", Ext = docExt,
                Set = (Action<string>)(p => model.TenthCertificatePath = p),
                Get = (Func<string?>)(() => model.TenthCertificatePath) },

            new { File = model.TwelthCertificate, Name = "12th Certificate", Ext = docExt,
                Set = (Action<string>)(p => model.TwelthCertificatePath = p),
                Get = (Func<string?>)(() => model.TwelthCertificatePath) },

            new { File = model.GraduationCertificate, Name = "Graduation Certificate", Ext = docExt,
                Set = (Action<string>)(p => model.GraduationCertificatePath = p),
                Get = (Func<string?>)(() => model.GraduationCertificatePath) },

            new { File = model.MastersCertificate, Name = "Masters Certificate", Ext = docExt,
                Set = (Action<string>)(p => model.MastersCertificatePath = p),
                Get = (Func<string?>)(() => model.MastersCertificatePath) },
        };

                // ================= STEP 1: FILE VALIDATION =================
                foreach (var f in files)
                {
                    // 🔥 FILE NULL → SKIP
                    if (f.File == null)
                        continue;

                    if (f.File.Length > maxSize)
                    {
                        return BadRequest(new
                        {
                            status = false,
                            statusCode = 400,
                            message = $"{f.Name} size should not exceed 2 MB"
                        });
                    }

                    try
                    {
                        var path = _fileUploadService.ValidateAndGeneratePath(
                            f.File,
                            "EmployeeFiles",
                            f.Ext
                        );

                        f.Set(path);
                    }
                    catch (Exception ex)
                    {
                        return BadRequest(new
                        {
                            status = false,
                            statusCode = 400,
                            message = $"{f.Name} error: {ex.Message}"
                        });
                    }
                }

                // ================= STEP 2: DB UPDATE =================
                bool isUpdated = await _services.UpdateEmployeeRegistrationData(model);

                if (!isUpdated)
                {
                    return BadRequest(new
                    {
                        status = false,
                        statusCode = 400,
                        message = "Employee update failed. Please check entered details."
                    });
                }

                // ================= STEP 3: FILE UPLOAD =================
                foreach (var f in files)
                {
                    if (f.File == null)
                        continue;

                    try
                    {
                        await _fileUploadService.UploadAsync(f.File, f.Get()!);
                    }
                    catch (Exception ex)
                    {
                        return StatusCode(500, new
                        {
                            status = false,
                            statusCode = 500,
                            message = $"Upload failed for {f.Name}: {ex.Message}"
                        });
                    }
                }

                return Ok(new
                {
                    status = true,
                    statusCode = 200,
                    message = "Employee updated successfully"
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    status = false,
                    statusCode = 500,
                    message = "Internal server error. Please try again later."
                });
            }
        }

        #endregion

        #region leave api

        [HttpPost("insertLeave")]
        public async Task<IActionResult> insertLeave([FromBody] Leave data)
        {
            try
            {
                int result = await _services.InsertLeave(data);

                if (result == 1)
                {
                    return Ok(new
                    {
                        status = true,
                        statusCode = 200,
                        message = "Leave Inserted Successfully!!"
                    });
                }

                if (result == 2)
                {
                    return Ok(new
                    {
                        status = true,
                        statusCode = 200,
                        message = "Leave Updated Successfully!!"
                    });
                }

                if (result == -1)
                {
                    return Ok(new
                    {
                        status = false,
                        statusCode = 400,
                        message = "Leave already exists!!"
                    });
                }

                return Ok(new
                {
                    status = false,
                    statusCode = 400,
                    message = "Some error occured during leave insertion!!"
                });
            }
            catch (Exception)
            {
                return Ok(new
                {
                    status = false,
                    statusCode = 500,
                    message = "Internal Server error!!"
                });
            }
        }

        [HttpGet("getLeaveById")]
        public async Task<IActionResult> getLeaveById(int id)
        {
            try
            {

                var result = await _services.getAllLeaveById(id);


                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<Leave>.FailureResponse(
                    "An error occurred while fetching role.",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }

        [HttpDelete("DeleteLeaveById")]
        public async Task<IActionResult> deleteLeaveById(int id)
        {
            try
            {
                bool res = await _services.deleteLeaveById(id);
                if (res)
                {
                    return Ok(new
                    {
                        status = true,
                        StatusCode = 200,
                        message = "Leave deleted successfully!!"
                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = false,
                        StatusCode = 400,
                        messsag = "Leave not deleted!!"
                    });
                }
            }
            catch
            {

                return Ok(new
                {
                    StatusCode = 500,
                    status = false,
                    message = "Interval server error!!"
                });
            }
        }

        [HttpPost("AssignLeave")]
        public async Task<IActionResult> AssignLeave([FromBody] AssignLeave model)
        {
            try
            {
                var result = await _services.InsertAssignLeaveAsync(model);

                if (!result)
                {
                    return BadRequest(new
                    {
                        status = false,
                        message = "Leave not assigned!!"
                    });
                }

                return Ok(new
                {
                    status = true,
                    message = "Leave assigned successfully!!"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    status = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    status = false,
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }

        [HttpGet("getAssignedLeaveById")]
        public async Task<IActionResult> getAssignedLeaveById(int empId)
        {
            try
            {

                var result = await _services.getAllAssignedLeaveById(empId);


                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<showLeave>.FailureResponse(
                    "An error occurred while fetching assigned leave.",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }

        [HttpGet("getAllAssignLeave")]
        public async Task<IActionResult> getAllAssignLeave(string? searchTerm = null, int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                ApiResponse<List<allAssignedLeave>> result =
                    await _services.getAllAssignedLeave(searchTerm, pageNumber, pageSize);


                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AssignLeaveDetails>>.FailureResponse(
                    "An error occurred while fetching leave",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }

        [HttpGet("getAllLeaveRequests")]
        public async Task<IActionResult> GetLeave(string? searchTerm = null, int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                ApiResponse<List<applyLeave>> result =
                    await _services.GetAllLeaveRequests(searchTerm, pageNumber, pageSize);
                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<applyLeave>>.FailureResponse(
                    "An error occurred while fetching leave",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }
        [HttpPut("updateLeaveStatus")]
        public async Task<IActionResult> UpdateLeaveStatus(int id, int status, string reason = null)
        {
            try
            {
                bool result = await _services.UpdateLeaveRequest(id, status, reason);

                if (result)
                {
                    return Ok(new
                    {
                        status = true,
                        statusCode = 200,
                        message = status == 1 ? "Approved Sucessfully" : "Rejected Successfully"
                    });
                }
                return Ok(new
                {
                    status = false,
                    statusCode = 400,
                    message = "Some error occured during updating!!"
                });
            }
            catch (Exception)
            {
                return Ok(new
                {
                    status = false,
                    statusCode = 500,
                    message = "Internal Server error!!"
                });
            }
        }
        #endregion

        #region attendance 

        [HttpPost("uploadAttendance")]
        public async Task<IActionResult> UploadAttendance([FromForm] AttendanceUploadRequest request)
        {
            if (request.File == null)
                return BadRequest("Excel file required");

            int inserted = await _services.UploadAttendance(request.File, request.Month, request.Year);

            if (inserted > 0)
            {
                return Ok(new
                {
                    status = true,
                    statusCode = 200,
                    message = "Attendance uploaded successfully",
                    insertedRecords = inserted
                });
            }
            else
            {
                return Ok(new
                {
                    status = false,
                    statusCode = 400,
                    message = "Some error occured!!"

                });
            }
        }

        [HttpGet("EmpAttendancebyEmpCode")]
        public async Task<IActionResult> GetAttendanceByEmpCode([FromQuery] string empCode, [FromQuery] int month, [FromQuery] int year)
        {
            if (string.IsNullOrWhiteSpace(empCode))
                return BadRequest(ApiResponse<List<Attendance>>.FailureResponse(
                    "Employee code is required",
                    400,
                    errorCode: "EMP_CODE_REQUIRED"
                ));

            var result = await _services.EmpAttendanceDataByEmpCode(empCode, month, year);

            return StatusCode(result.StatusCode, result);
        }


        [HttpGet("EmpMonthlyWorkingDetailByEmpCode")]
        public async Task<IActionResult> EmpMonthlyWorkingDetailByEmpCode(int empCode, int month, int year)
        {
            try
            {
                var result = await _services.EmpWorkingDetailsByempCode(empCode, month, year);

                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<EmpWorkingDetails>.FailureResponse(
                    "An error occurred while fetching employee work details.",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }
        #endregion

        #region add project


        [HttpPost("add-update-Project")]
        public async Task<IActionResult> AddOrUpdateProject([FromForm] project data)
        {
            string[] allowedExtensions = { ".pdf", ".doc", ".docx" };

            if (string.IsNullOrWhiteSpace(data.projectTitle))
                return BadRequest("Project title is required.");

            if (data.sopDocument == null && data.id <= 0)
                if (data.sopDocument == null && data.id <= 0)
                    return BadRequest("SOW document is required.");

            if (data.startDate == null)
                return BadRequest("Start date is required.");

            if (data.assignDate == null)
                return BadRequest("Assign date is required.");

            if (data.assignDate < data.startDate)
                return BadRequest("Assign date cannot be less than start date.");

            if (data.endDate != null &&
                (data.endDate <= data.startDate || data.endDate <= data.assignDate))
                return BadRequest("End date must be greater than start date and assign date.");

            if (data.completionDate != null &&
                (data.completionDate <= data.startDate || data.completionDate <= data.assignDate))
                return BadRequest("Completion date must be greater than start date and assign date.");

            if (data.category != null &&
                data.category.Equals("Software", StringComparison.OrdinalIgnoreCase))
            {
                //bool isAnySoftwareSelected =
                //    !string.IsNullOrWhiteSpace(data.web) ||
                //    !string.IsNullOrWhiteSpace(data.app) ||
                //    !string.IsNullOrWhiteSpace(data.androidApp) ||
                //    !string.IsNullOrWhiteSpace(data.IOSApp);

                bool isAnySoftwareSelected = data.web || data.app || data.androidApp || data.IOSApp;

                if (!isAnySoftwareSelected)
                    return BadRequest("Please select at least one software type.");
            }

            try
            {
                if (data.sopDocument != null)
                {
                    data.sopDocumentPath = _fileUploadService.ValidateAndGeneratePath(
                        data.sopDocument,
                        "ProjectDocuments",
                        allowedExtensions
                    );
                }

                if (data.technicalDocument != null)
                {
                    data.technicalDocumentPath = _fileUploadService.ValidateAndGeneratePath(
                        data.technicalDocument,
                        "ProjectDocuments",
                        allowedExtensions
                    );
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Invalid file type . Upload only .pdf, .doc, .docx type file!!"
                });
            }

            bool result = await _services.AddProject(data);

            if (!result)
            {
                return Ok(new
                {
                    success = false,
                    message = "Project not saved."
                });
            }

            if (data.sopDocument != null)
                await _fileUploadService.UploadAsync(data.sopDocument, data.sopDocumentPath!);

            if (data.technicalDocument != null)
                await _fileUploadService.UploadAsync(
                    data.technicalDocument,
                    data.technicalDocumentPath!
                );

            return Ok(new
            {
                success = true,
                message = data.id > 0
                    ? "Project updated successfully."
                    : "Project added successfully."
            });
        }


        //[HttpGet("getAllProject")]
        //public async Task<IActionResult> getAllProject(string? searchTerm = null, int? pageNumber = null, int? pageSize = null, string? status = null)
        //{
        //    try
        //    {
        //        ApiResponse<List<project>> result =
        //            await _services.GetAllProject(searchTerm, pageNumber, pageSize,status);


        //        return StatusCode(result.StatusCode, result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ApiResponse<List<project>>.FailureResponse(
        //            "An error occurred while fetching project details",
        //            500,
        //            "SERVER_ERROR"
        //        ));
        //    }
        //}

        [HttpGet("getProjectDetailsByEmpId")]
        public async Task<IActionResult> getProjectDetailsById(int empId)
        {
            try
            {
                ApiResponse<List<project>> result =
                    await _services.GetEmpProjectDetailByEmpId(empId);


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
        [HttpGet("getAllProjectById")]
        public async Task<IActionResult> getAllProjectById(int id)
        {
            try
            {
                ApiResponse<List<project>> result =
                    await _services.GetAllProjectById(id);


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


        [HttpDelete("deleteProjectById")]
        public async Task<IActionResult> deleteProjectById(int id)
        {
            try
            {
                var res = await _services.deleteProjectById(id);
                if (res)
                {
                    return Ok(new
                    {
                        status = true,
                        StatusCode = 200,
                        message = "Project deleted successfully!!!"

                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = false,
                        StatusCode = 404,
                        message = "Project not deleted!!"
                    });
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    status = false,
                    StatusCode = 500,
                    message = "An error occurred while deleting Project.",
                    error = ex.Message
                });
            }
        }

        [HttpPost("insert-update-Task")]
        public async Task<IActionResult> insertUpdateTask([FromForm] Taskassign data)
        {
            if (data == null)
            {
                return BadRequest(new
                {
                    status = false,
                    statusCode = 400,
                    message = "Invalid request data."
                });
            }

            try
            {
                string[] docExt = { ".pdf", ".jpg", ".jpeg", ".png" };

                // ---------- FILE CONFIG (INLINE) ----------
                var files = new[]
                {
            new { File = data.document1, Folder = "TaskDocuments", Ext = docExt, Set = (Action<string>)(p => data.document1Path = p), Get = (Func<string?>)(() => data.document1Path) },
            new { File = data.document2, Folder = "TaskDocuments", Ext = docExt, Set = (Action<string>)(p => data.document2Path = p), Get = (Func<string?>)(() => data.document2Path) },
        };

                // ---------- STEP 1: VALIDATE + GENERATE PATH ----------
                foreach (var f in files)
                {
                    if (f.File == null) continue;

                    f.Set(_fileUploadService.ValidateAndGeneratePath(
                        f.File,
                        f.Folder,
                        f.Ext
                    ));
                }

                // ---------- STEP 2: DB INSERT / UPDATE ----------
                data.assignedBy = _userId;

                bool res = await _services.insertTask(data);

                if (!res)
                {
                    return BadRequest(ApiResponse<object>.FailureResponse(
                        data.id > 0
                            ? "Some error occurred while updating Task and assign"
                            : "Some error occurred while saving Task Create & Assign response",
                        400
                    ));
                }

                // ---------- STEP 3: ACTUAL FILE UPLOAD ----------
                foreach (var f in files)
                {
                    if (f.File != null)
                        await _fileUploadService.UploadAsync(f.File, f.Get()!);
                }

                return Ok(ApiResponse<object>.SuccessResponse(
                    null,
                    data.id > 0
                        ? "Task update & assign successfully"
                        : "Task Create & Assign inserted successfully"
                ));
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.FailureResponse(
                    $"Internal server error: {ex.Message}",
                    500
                ));
            }
        }


        [HttpGet("getAllAssignTask")]
        public async Task<IActionResult> getAllAssignTask(string? searchTerm = null, int? pageNumber = null, int? pageSize = null)
        {
            try
            {
                ApiResponse<List<TaskAssignDto>> result =
                    await _services.getAllAssignTask(searchTerm, pageNumber, pageSize);


                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<Taskassign>>.FailureResponse(
                    "An error occurred while fetching assign task details",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }


        [HttpGet("getAllTaskById")]
        public async Task<IActionResult> getAllTaskById(int id)
        {
            try
            {
                ApiResponse<List<Taskassign>> result =
                    await _services.getAllAssignTaskById(id);


                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<Taskassign>>.FailureResponse(
                    "An error occurred while fetching project details",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }


        [HttpDelete("deleteTaskAssignById")]
        public async Task<IActionResult> deleteTaskAssignById(int id)
        {
            try
            {
                var res = await _services.deleteTaskById(id);
                if (res)
                {
                    return Ok(new
                    {
                        status = true,
                        StatusCode = 200,
                        message = "Assign task deleted successfully!!!"

                    });
                }
                else
                {
                    return Ok(new
                    {
                        status = false,
                        StatusCode = 404,
                        message = "Assign Task not deleted!!"
                    });
                }
            }
            catch (Exception ex)
            {

                return StatusCode(500, new
                {
                    status = false,
                    StatusCode = 500,
                    message = "An error occurred while deleting task.",
                    error = ex.Message
                });
            }
        }

        #endregion

        #region Admin dashboard

        [HttpGet("AdminDashboardCount")]
        public async Task<IActionResult> AdminDashboardCount()
        {
            try
            {
                ApiResponse<List<AdminDashboardCountDto>> result =
                    await _services.adminDashboardCount();


                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AdminDashboardCountDto>>.FailureResponse(
                    "An error occurred while fetching admin dashboard count",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }
        #endregion


        #region assignedProjectEmpList

        [HttpGet("ProjectCoOrdinateList")]
        public async Task<IActionResult> AllEmpListAssignedProject(int projectId,int pmId)
        {
            try
            {
                ApiResponse<List<AssignedProjectEmpListDto>> result =
                    await _services.assignedProjectEmpList(projectId, pmId);


                return StatusCode(result.StatusCode, result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<List<AssignedProjectEmpListDto>>.FailureResponse(
                    "An error occurred while fetching all emp list assigned project",
                    500,
                    "SERVER_ERROR"
                ));
            }
        }
        #endregion
    }
}
