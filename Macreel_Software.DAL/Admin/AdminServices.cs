using Macreel_Software.Contracts.DTOs;
using Macreel_Software.Models;
using Macreel_Software.Models.Employee;
using Macreel_Software.Models.Master;
using Macreel_Software.Services.AttendanceUpload;
using Macreel_Software.Services.MailSender;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Text.Json;

namespace Macreel_Software.DAL.Admin
{
    public  class AdminServices:IAdminServices
    {
        private readonly SqlConnection _conn;
        private readonly UploadAttendance _upload;
        private readonly PasswordEncrypt _pass;

        public AdminServices(IConfiguration config, UploadAttendance load, PasswordEncrypt pass)
        {
            _conn = new SqlConnection(
                config.GetConnectionString("DefaultConnection"));
            _upload=load;
            _pass = pass;
        }

        #region employee registration
        public async Task<string> InsertEmployeeRegistrationData(employeeRegistration data)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_Employee", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@action", "insert");

                    // --- Employee Details ---
                    cmd.Parameters.AddWithValue("@empRole", data.EmpRoleId);
                    cmd.Parameters.AddWithValue("@empCode", data.EmpCode);
                    cmd.Parameters.AddWithValue("@empName", data.EmpName);
                    cmd.Parameters.AddWithValue("@mobile", data.Mobile);
                    cmd.Parameters.AddWithValue("@department", data.DepartmentId);
                    cmd.Parameters.AddWithValue("@designation", data.DesignationId);
                    cmd.Parameters.AddWithValue("@reportingManager",
                        (object?)data.ReportingManagerId ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@profilePic", data.ProfilePicPath ?? "");
                    cmd.Parameters.AddWithValue("@aadharImg", data.AadharImgPath ?? "");
                    cmd.Parameters.AddWithValue("@adharBackImg", data.AadharBackImgPath ?? "");
                    cmd.Parameters.AddWithValue("@panImg", data.PanImgPath ?? "");
                    cmd.Parameters.AddWithValue("@panBackImg", data.PanBackImgPath ?? "");

                    cmd.Parameters.AddWithValue("@emailId", data.EmailId);
                    cmd.Parameters.AddWithValue("@dateOfJoining", data.DateOfJoining);
                    cmd.Parameters.AddWithValue("@salary", data.Salary);
                    cmd.Parameters.AddWithValue("@password", data.Password);

                    cmd.Parameters.AddWithValue("@bankName", data.BankName);
                    cmd.Parameters.AddWithValue("@accountNo", data.AccountNo);
                    cmd.Parameters.AddWithValue("@ifscCode", data.IfscCode);
                    cmd.Parameters.AddWithValue("@bankBranch", data.BankBranch);

                    cmd.Parameters.AddWithValue("@dob", data.Dob);
                    cmd.Parameters.AddWithValue("@gender", data.Gender);
                    cmd.Parameters.AddWithValue("@nationality", data.Nationality);
                    cmd.Parameters.AddWithValue("@maritalStatus", data.MaritalStatus);

                    cmd.Parameters.AddWithValue("@presentAddress", data.PresentAddress);
                    cmd.Parameters.AddWithValue("@state", data.StateId);
                    cmd.Parameters.AddWithValue("@city", data.CityId);
                    cmd.Parameters.AddWithValue("@pincode", data.Pincode);

                    cmd.Parameters.AddWithValue("@emergencyContactPersonName", data.EmergencyContactPersonName);
                    cmd.Parameters.AddWithValue("@emergenctContactNum", data.EmergencyContactNum);

                    // --- Previous Experience ---
                    cmd.Parameters.AddWithValue("@companyName", data.CompanyName ?? "");
                    cmd.Parameters.AddWithValue("@yearOfExperience", data.YearOfExperience);
                    cmd.Parameters.AddWithValue("@technology", data.Technology ?? "");
                    cmd.Parameters.AddWithValue("@companyContactno", data.CompanyContactNo ?? "");
                    cmd.Parameters.AddWithValue("@experienceCertificate", data.ExperienceCertificatePath ?? "");

                    // --- Certificates ---
                    cmd.Parameters.AddWithValue("@tenthCertificate", data.TenthCertificatePath ?? "");
                    cmd.Parameters.AddWithValue("@twelthCertificate", data.TwelthCertificatePath ?? "");
                    cmd.Parameters.AddWithValue("@graduationCertificate", data.GraduationCertificatePath ?? "");
                    cmd.Parameters.AddWithValue("@mastersCertificate", data.MastersCertificatePath ?? "");

                    cmd.Parameters.AddWithValue("@addedBy", data.addedBy);

                    // ✅ Convert comma-separated SkillIdsCsv to DataTable for TVP
                    DataTable dtSkills = new DataTable();
                    dtSkills.Columns.Add("technolgyId", typeof(int));

                    if (!string.IsNullOrEmpty(data.SkillIds))
                    {
                        var ids = data.SkillIds.Split(',')
                                                  .Select(s => int.Parse(s.Trim()));

                        foreach (var id in ids)
                            dtSkills.Rows.Add(id);
                    }

                    var param = cmd.Parameters.AddWithValue("@TechnologyIds", dtSkills);
                    param.SqlDbType = SqlDbType.Structured;
                    param.TypeName = "dbo.TechnologyTableType";

                    if (_conn.State == ConnectionState.Closed)
                        await _conn.OpenAsync();

                    using (SqlDataReader dr = await cmd.ExecuteReaderAsync())
                    {
                        if (await dr.ReadAsync())
                            return dr["message"].ToString();
                    }
                }

                return "Employee registration failed";
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    _conn.Close();
            }
        }


        public async Task<List<ReportingManger>> GetAllReportingManager()
        {
            List<ReportingManger> list = new();

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_employee", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action" ,"getReportingManager");

                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await sdr.ReadAsync())
                        {
                            list.Add(new ReportingManger
                            {
                                id = Convert.ToInt32(sdr["id"]),
                                ReportingManagerName = sdr["empName"]?.ToString()
                            });
                        }
                    }
                }
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }

            return list;
        }


        public async Task<ApiResponse<List<employeeRegistration>>> GetAllEmpData(string? searchTerm,int? pageNumber,int? pageSize, string? addedBy)
        {
            List<employeeRegistration> list = new List<employeeRegistration>();
            int totalRecords = 0;

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_employee", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "getAllEmployeeDetails");

                    cmd.Parameters.AddWithValue("@searchTerm",
                        string.IsNullOrWhiteSpace(searchTerm) ? DBNull.Value : searchTerm);
                    cmd.Parameters.AddWithValue("@addedBy",addedBy);

                    cmd.Parameters.AddWithValue("@pageNumber",
                        pageNumber.HasValue ? pageNumber.Value : DBNull.Value);

                    cmd.Parameters.AddWithValue("@pageSize",
                        pageSize.HasValue ? pageSize.Value : DBNull.Value);

                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await sdr.ReadAsync())
                        {
                            if (totalRecords == 0 && sdr["TotalRecords"] != DBNull.Value)
                                totalRecords = Convert.ToInt32(sdr["TotalRecords"]);

                            list.Add(new employeeRegistration
                            {
                                Id = Convert.ToInt32(sdr["id"]),
                                EmpCode = sdr["empCode"] != DBNull.Value ? Convert.ToInt32(sdr["empCode"]) : (int?)null,
                                EmpRoleId = sdr["empRole"] != DBNull.Value ? Convert.ToInt32(sdr["empRole"]) : (int?)null,
                                roleName = sdr["roleName"]?.ToString(),
                                EmpName = sdr["empName"]?.ToString(),
                                Mobile = sdr["mobile"]?.ToString(),
                                DepartmentId = sdr["department"] != DBNull.Value ? Convert.ToInt32(sdr["department"]) : (int?)null,
                                departmentName = sdr["departmentName"]?.ToString(),
                                DesignationId = sdr["designation"] != DBNull.Value ? Convert.ToInt32(sdr["designation"]) : (int?)null,
                                designationName = sdr["designationName"]?.ToString(),
                                ReportingManagerId = sdr["reportingManager"] != DBNull.Value ? Convert.ToInt32(sdr["reportingManager"]) : (int?)null,
                                AadharImgPath = sdr["aadharImg"]?.ToString(),
                                AadharBackImgPath = sdr["adharBackImg"]?.ToString(),
                                PanBackImgPath = sdr["panBackImg"]?.ToString(),
                                PanImgPath = sdr["panImg"]?.ToString(),
                                EmailId = sdr["emailId"]?.ToString(),
                                DateOfJoining = sdr["dateOfJoining"] != DBNull.Value ? Convert.ToDateTime(sdr["dateOfJoining"]) : DateTime.MinValue,
                                Salary = sdr["salary"] != DBNull.Value ? Convert.ToInt32(sdr["salary"]) : (int?)null,
                                ProfilePicPath = sdr["profilePic"]?.ToString(),
                                BankName = sdr["bankName"]?.ToString(),
                                AccountNo = sdr["accountNo"]?.ToString(),
                                IfscCode = sdr["ifscCode"]?.ToString(),
                                BankBranch = sdr["bankBranch"]?.ToString(),
                                Dob = sdr["dob"] != DBNull.Value ? Convert.ToDateTime(sdr["dob"]) : DateTime.MinValue,
                                Gender = sdr["gender"]?.ToString(),
                                Nationality = sdr["nationality"]?.ToString(),
                                MaritalStatus = sdr["maritalStatus"]?.ToString(),
                                PresentAddress = sdr["presentAddress"]?.ToString(),
                                StateId = sdr["state"] != DBNull.Value ? Convert.ToInt32(sdr["state"]) : (int?)null,
                                CityId = sdr["city"] != DBNull.Value ? Convert.ToInt32(sdr["city"]) : (int?)null,
                                Pincode = sdr["pincode"]?.ToString(),
                                EmergencyContactPersonName = sdr["emergencyContactPersonName"]?.ToString(),
                                EmergencyContactNum = sdr["emergenctContactNum"]?.ToString(),
                                CompanyName = sdr["companyName"] != DBNull.Value ? sdr["companyName"]?.ToString() : null,
                                Technology = sdr["previousExp"] != DBNull.Value ? sdr["previousExp"]?.ToString() : null,
                                CompanyContactNo = sdr["companyContactNo"] != DBNull.Value ? sdr["companyContactNo"]?.ToString() : null,
                                ExperienceCertificatePath = sdr["experienceCertificate"] != DBNull.Value ? sdr["experienceCertificate"]?.ToString() : null,
                                TenthCertificatePath = sdr["tenthCertificate"] != DBNull.Value ? sdr["tenthCertificate"]?.ToString() : null,
                                TwelthCertificatePath = sdr["twelthCertificate"] != DBNull.Value ?
                                sdr["twelthCertificate"]?.ToString() : null,

                                GraduationCertificatePath = sdr["graduationCertificate"] != DBNull.Value ?
                                sdr["graduationCertificate"]?.ToString() : null,
                                Password = sdr["Password"] != DBNull.Value? _pass.DecryptPassword(sdr["Password"]?.ToString()!):null,
                                MastersCertificatePath = sdr["mastersCertificate"] != DBNull.Value ? sdr["mastersCertificate"]?.ToString() : null,
                                YearOfExperience = sdr["yearOfExperience"] != DBNull.Value ? Convert.ToInt32(sdr["yearOfExperience"]) : (int?)null,
                                stateName = sdr["stateName"] != DBNull.Value ? sdr["stateName"].ToString() : null,
                                cityName = sdr["cityName"] != DBNull.Value ? sdr["cityName"].ToString() : null,
                                skill = sdr["skillsJson"] != DBNull.Value
                                ? JsonSerializer.Deserialize<List<Skill>>(sdr["skillsJson"].ToString()!)
                                : new List<Skill>(),
                                ReportingManagerName = sdr["ReportingManager"] != DBNull.Value ? sdr["ReportingManager"].ToString():null,
                            });
                        }
                    }
                }

                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    return ApiResponse<List<employeeRegistration>>.PagedResponse(
                        list,
                        pageNumber.Value,
                        pageSize.Value,
                        totalRecords,
                        "Employee data list fetched successfully");
                }

                var response = ApiResponse<List<employeeRegistration>>.SuccessResponse(
                    list,
                    "Employee data list fetched successfully");

                response.TotalRecords = totalRecords;

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponse<List<employeeRegistration>>.FailureResponse(
                    "Failed to fetch employee data",
                    500,
                    "EMP_FETCH_ERROR");
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }
        public async Task<bool> deleteEmployeeById(int id)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_employee", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "deleteEmployeeById");
                    cmd.Parameters.AddWithValue("@id", id);

                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    int rowsAffected = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                    return rowsAffected > 0;
                }
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }



        public async Task<ApiResponse<List<employeeRegistration>>> GetAllEmpDataById(int id)
        {
            List<employeeRegistration> list = new List<employeeRegistration>();
            int totalRecords = 0;
           string encryptedPassword = null;
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_employee", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "getEmployeeById");
                    cmd.Parameters.AddWithValue("@id", id);

                   

                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await sdr.ReadAsync())
                        {
                            encryptedPassword = sdr["password"] != DBNull.Value? sdr["password"].ToString(): null;

                            string decryptedPassword = null;

                            if (!string.IsNullOrEmpty(encryptedPassword))
                            {
                                decryptedPassword =  _pass.DecryptPassword(encryptedPassword);
                            }
                            if (totalRecords == 0 && sdr["TotalRecords"] != DBNull.Value)totalRecords = Convert.ToInt32(sdr["TotalRecords"]);

                            list.Add(new employeeRegistration
                            {
                                Id = Convert.ToInt32(sdr["id"]),
                                EmpCode = sdr["empCode"] != DBNull.Value ? Convert.ToInt32(sdr["empCode"]) : (int?)null,
                                EmpRoleId = sdr["empRole"] != DBNull.Value ? Convert.ToInt32(sdr["empRole"]) : (int?)null,
                                roleName = sdr["roleName"]?.ToString(),
                                EmpName = sdr["empName"]?.ToString(),
                                Mobile = sdr["mobile"]?.ToString(),
                                DepartmentId = sdr["department"] != DBNull.Value ? Convert.ToInt32(sdr["department"]) : (int?)null,
                                departmentName = sdr["departmentName"]?.ToString(),
                                DesignationId = sdr["designation"] != DBNull.Value ? Convert.ToInt32(sdr["designation"]) : (int?)null,
                                designationName = sdr["designationName"]?.ToString(),
                                ReportingManagerId = sdr["reportingManager"] != DBNull.Value ? Convert.ToInt32(sdr["reportingManager"]) : (int?)null,
                                AadharImgPath = sdr["aadharImg"]?.ToString(),
                                PanImgPath = sdr["panImg"]?.ToString(),
                                EmailId = sdr["emailId"]?.ToString(),
                                DateOfJoining = sdr["dateOfJoining"] != DBNull.Value ? Convert.ToDateTime(sdr["dateOfJoining"]) : DateTime.MinValue,
                                Salary = sdr["salary"] != DBNull.Value ? Convert.ToInt32(sdr["salary"]) : (int?)null,
                                ProfilePicPath = sdr["profilePic"]?.ToString(),
                                BankName = sdr["bankName"]?.ToString(),
                                AccountNo = sdr["accountNo"]?.ToString(),
                                IfscCode = sdr["ifscCode"]?.ToString(),
                                BankBranch = sdr["bankBranch"]?.ToString(),
                                Dob = sdr["dob"] != DBNull.Value ? Convert.ToDateTime(sdr["dob"]) : DateTime.MinValue,
                                Gender = sdr["gender"]?.ToString(),
                                Nationality = sdr["nationality"]?.ToString(),
                                MaritalStatus = sdr["maritalStatus"]?.ToString(),
                                PresentAddress = sdr["presentAddress"]?.ToString(),
                                StateId = sdr["state"] != DBNull.Value ? Convert.ToInt32(sdr["state"]) : (int?)null,
                                CityId = sdr["city"] != DBNull.Value ? Convert.ToInt32(sdr["city"]) : (int?)null,
                                Pincode = sdr["pincode"]?.ToString(),
                                EmergencyContactPersonName = sdr["emergencyContactPersonName"]?.ToString(),
                                EmergencyContactNum = sdr["emergenctContactNum"]?.ToString(),
                                CompanyName = sdr["city"] != DBNull.Value ? sdr["companyName"]?.ToString() : null,
                                Technology = sdr["technology"] != DBNull.Value ? sdr["technology"]?.ToString() : null,
                                CompanyContactNo = sdr["companyContactNo"] != DBNull.Value ? sdr["companyContactNo"]?.ToString() : null,
                                ExperienceCertificatePath = sdr["experienceCertificate"] != DBNull.Value ? sdr["experienceCertificate"]?.ToString() : null,
                                TenthCertificatePath = sdr["tenthCertificate"] != DBNull.Value ? sdr["tenthCertificate"]?.ToString() : null,
                                TwelthCertificatePath = sdr["twelthCertificate"] != DBNull.Value ?
                                sdr["twelthCertificate"]?.ToString() : null,

                                GraduationCertificatePath = sdr["graduationCertificate"] != DBNull.Value ?
                                sdr["graduationCertificate"]?.ToString() : null,

                                MastersCertificatePath = sdr["mastersCertificate"] != DBNull.Value ? sdr["mastersCertificate"]?.ToString() : null,
                                YearOfExperience = sdr["yearOfExperience"] != DBNull.Value ? Convert.ToInt32(sdr["yearOfExperience"]) : (int?)null,
                                stateName = sdr["stateName"] != DBNull.Value ? sdr["stateName"].ToString() : null,
                                cityName = sdr["cityName"] != DBNull.Value ? sdr["cityName"].ToString() : null,
                                skill = sdr["skillsJson"] != DBNull.Value
                ? JsonSerializer.Deserialize<List<Skill>>(sdr["skillsJson"].ToString())
                : new List<Skill>(),
                                Password = decryptedPassword
                            });
                         
                        }
                    }
                }

                if (!list.Any())
                {
                    return ApiResponse<List<employeeRegistration>>.FailureResponse(
                        "Employee not found",
                        404,
                        "EMPLOYEE_NOT_FOUND"
                    );
                }


                return ApiResponse<List<employeeRegistration>>.SuccessResponse(
                    list,
                    "Employee fetched successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<List<employeeRegistration>>.FailureResponse(
                    ex.Message,
                    500,
                    "EMPLOYEE_FETCH_ERROR"
                );
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }


        public async Task<bool> UpdateEmployeeRegistrationData(employeeRegistration data)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_Employee", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "updateEmployee");

                    cmd.Parameters.AddWithValue("@id", data.Id);
                    cmd.Parameters.AddWithValue("@empRole", data.EmpRoleId);
                    cmd.Parameters.AddWithValue("@empCode", data.EmpCode);
                    cmd.Parameters.AddWithValue("@empName", data.EmpName);
                    cmd.Parameters.AddWithValue("@mobile", data.Mobile);
                    cmd.Parameters.AddWithValue("@department", data.DepartmentId);
                    cmd.Parameters.AddWithValue("@designation", data.DesignationId);
                    cmd.Parameters.AddWithValue("@reportingManager",
                        (object?)data.ReportingManagerId ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@profilePic", (object?)data.ProfilePicPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@aadharImg", (object?)data.AadharImgPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@panImg", (object?)data.PanImgPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@adharBackImg", (object?)data.AadharBackImgPath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("panBackImg", (object?)data.PanBackImgPath ?? DBNull.Value);

                    cmd.Parameters.AddWithValue("@dateOfJoining", data.DateOfJoining);
                    cmd.Parameters.AddWithValue("@salary", data.Salary);

                    cmd.Parameters.AddWithValue("@bankName", data.BankName);
                    cmd.Parameters.AddWithValue("@accountNo", data.AccountNo);
                    cmd.Parameters.AddWithValue("@ifscCode", data.IfscCode);
                    cmd.Parameters.AddWithValue("@bankBranch", data.BankBranch);

                    cmd.Parameters.AddWithValue("@dob", data.Dob);
                    cmd.Parameters.AddWithValue("@gender", data.Gender);
                    cmd.Parameters.AddWithValue("@nationality", data.Nationality);
                    cmd.Parameters.AddWithValue("@maritalStatus", data.MaritalStatus);

                    cmd.Parameters.AddWithValue("@presentAddress", data.PresentAddress);
                    cmd.Parameters.AddWithValue("@state", data.StateId);
                    cmd.Parameters.AddWithValue("@city", data.CityId);
                    cmd.Parameters.AddWithValue("@pincode", data.Pincode);

                    cmd.Parameters.AddWithValue("@emergencyContactPersonName", data.EmergencyContactPersonName);
                    cmd.Parameters.AddWithValue("@emergenctContactNum", data.EmergencyContactNum);

                    cmd.Parameters.AddWithValue("@companyName", data.CompanyName);
                    cmd.Parameters.AddWithValue("@yearOfExperience", data.YearOfExperience);
                    cmd.Parameters.AddWithValue("@technology", data.Technology);
                    cmd.Parameters.AddWithValue("@companyContactno", data.CompanyContactNo);

                    cmd.Parameters.AddWithValue("@experienceCertificate",
                        (object?)data.ExperienceCertificatePath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@tenthCertificate",
                        (object?)data.TenthCertificatePath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@twelthCertificate",
                        (object?)data.TwelthCertificatePath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@graduationCertificate",
                        (object?)data.GraduationCertificatePath ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@mastersCertificate",
                        (object?)data.MastersCertificatePath ?? DBNull.Value);

                    // ✅ Convert comma-separated SkillIdsCsv to DataTable for TVP
                    //DataTable dtSkills = new DataTable();
                    //dtSkills.Columns.Add("technolgyId", typeof(int));

                    //if (!string.IsNullOrWhiteSpace(data.SkillIds))
                    //{
                    //    var ids = data.SkillIds
                    //                  .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    //                  .Select(s => int.TryParse(s.Trim(), out int v) ? v : 0)
                    //                  .Where(v => v > 0);

                    //    foreach (var id in ids)
                    //        dtSkills.Rows.Add(id);
                    //}

                    //var tvp = cmd.Parameters.Add("@TechnologyIds", SqlDbType.Structured);
                    //tvp.TypeName = "dbo.TechnologyTableType";
                    //tvp.Value = dtSkills;

                    if (_conn.State == ConnectionState.Closed)
                        await _conn.OpenAsync();

                    object result = await cmd.ExecuteScalarAsync();
                    int res = 0;
                    if (data != null && !string.IsNullOrEmpty(data.SkillIds) && Convert.ToInt32(result)>0)
                        res = await UpdateSkills(data.SkillIds, data.Id);
                    return Convert.ToBoolean(result);
                   

                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    _conn.Close();
            }
        }


        private async Task<int> UpdateSkills(string sk, int empId)
        {
            if (string.IsNullOrWhiteSpace(sk))
                return 0;

            int totalAffectedRows = 0;

            try
            {
                var ids = sk.Split(',').Select(s => int.Parse(s.Trim()));

                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();

                foreach (var id in ids)
                {
                    using (SqlCommand cmd = new SqlCommand("sp_employee", _conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("@action", "UpdateSkills");
                        cmd.Parameters.AddWithValue("@technologyId", id);
                        cmd.Parameters.AddWithValue("@id", empId);

                        totalAffectedRows += await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }

            return totalAffectedRows;
        }

        #endregion


        #region leave api

        public async Task<int> InsertLeave(Leave data)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_leave", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;


                    cmd.Parameters.AddWithValue("@leaveType", data.leaveName.Trim());
                    cmd.Parameters.AddWithValue("@description", data.description);
                    cmd.Parameters.AddWithValue("@id", data.Id);
                    cmd.Parameters.AddWithValue("@action", data.Id > 0 ? "updateLeave" : "insertLeave");


                    SqlParameter resultParam = new SqlParameter("@result", SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    cmd.Parameters.Add(resultParam);

                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    await cmd.ExecuteNonQueryAsync();

                    return Convert.ToInt32(resultParam.Value);
                }
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        public async Task<ApiResponse<List<Leave>>> getAllLeave(string? searchTerm,int? pageNumber,int? pageSize)
            {
            List<Leave> list = new();
            int totalRecords = 0;

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_leave", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectAllLeave");

                    cmd.Parameters.AddWithValue("@searchTerm",
                        string.IsNullOrWhiteSpace(searchTerm) ? DBNull.Value : searchTerm);

                    cmd.Parameters.AddWithValue("@pageNumber",
                        pageNumber.HasValue ? pageNumber.Value : DBNull.Value);

                    cmd.Parameters.AddWithValue("@pageSize",
                        pageSize.HasValue ? pageSize.Value : DBNull.Value);

                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await sdr.ReadAsync())
                        {

                            if (totalRecords == 0)
                                totalRecords = Convert.ToInt32(sdr["TotalRecords"]);

                            list.Add(new Leave
                            {
                                Id = Convert.ToInt32(sdr["id"]),
                                leaveName =sdr["leaveType"] !=DBNull.Value? sdr["leaveType"].ToString():null,
                                description =sdr["description"] !=DBNull.Value? sdr["description"].ToString():null,
                            });
                        }
                    }
                }


                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    return ApiResponse<List<Leave>>.PagedResponse(
                        list,
                        pageNumber.Value,
                        pageSize.Value,
                        totalRecords,
                        "Leave list fetched successfully");
                }


                var response = ApiResponse<List<Leave>>.SuccessResponse(
                    list,
                    "Leave list fetched successfully");

                response.TotalRecords = totalRecords;

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponse<List<Leave>>.FailureResponse(
                    ex.Message,
                    500,
                    "Leave_FETCH_ERROR");
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }


        public async Task<ApiResponse<List<allAssignedLeave>>> getAllAssignedLeave(string? searchTerm, int? pageNumber, int? pageSize)
        {
            List<allAssignedLeave> list = new();
            int totalRecords = 0;

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_assignLeave", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "getAllAssignedLeave");

                    cmd.Parameters.AddWithValue("@searchTerm",
                        string.IsNullOrWhiteSpace(searchTerm) ? DBNull.Value : searchTerm);

                    cmd.Parameters.AddWithValue("@pageNumber",
                        pageNumber.HasValue ? pageNumber.Value : DBNull.Value);

                    cmd.Parameters.AddWithValue("@pageSize",
                        pageSize.HasValue ? pageSize.Value : DBNull.Value);

                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await sdr.ReadAsync())
                        {

                            if (totalRecords == 0)
                                totalRecords = Convert.ToInt32(sdr["TotalRecords"]);

                            list.Add(new allAssignedLeave
                            {
                                //id = Convert.ToInt32(sdr["id"]),
                                EmpId = sdr["empId"] != DBNull.Value ? Convert.ToInt32(sdr["empId"]):null,
                                empName = sdr["empName"] != DBNull.Value ? sdr["empName"].ToString():null,
                                empCode = sdr["empCode"] != DBNull.Value ? Convert.ToInt32(sdr["empCode"]) : null,
                                designationId = sdr["designation"] != DBNull.Value ? Convert.ToInt32(sdr["designation"]) : null,
                                designationName = sdr["designationName"] != DBNull.Value ? sdr["designationName"].ToString() : null,
                                CLTotal = sdr["CLTotal"] != DBNull.Value ? Convert.ToInt32(sdr["CLTotal"]) : null,
                                CLRemaining = sdr["CLRemaining"] != DBNull.Value ? Convert.ToInt32(sdr["CLRemaining"]) : null,
                                CLUsed = sdr["CLUsed"] != DBNull.Value ? Convert.ToInt32(sdr["CLUsed"]) : null,
                                SLRemaining = sdr["SLRemaining"] != DBNull.Value ? Convert.ToInt32(sdr["SLRemaining"]) : null,
                                SLTotal = sdr["SLTotal"] != DBNull.Value ? Convert.ToInt32(sdr["SLTotal"]) : null,
                                SLUsed = sdr["SLUsed"] != DBNull.Value ? Convert.ToInt32(sdr["SLUsed"]) : null,
                                ELTotal = sdr["ELTotal"] != DBNull.Value ? Convert.ToInt32(sdr["ELTotal"]) : null,
                                ELRemaining = sdr["ELRemaining"] != DBNull.Value ? Convert.ToInt32(sdr["ELRemaining"]) : null,
                                ELUsed = sdr["ELUsed"] != DBNull.Value ? Convert.ToInt32(sdr["ELUsed"]) : null,
                                ELCarryForward = sdr["ELCarryForward"] != DBNull.Value ? Convert.ToInt32(sdr["ELCarryForward"]) : null,
                            });
                        }
                    }
                }


                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    return ApiResponse<List<allAssignedLeave>>.PagedResponse(
                        list,
                        pageNumber.Value,
                        pageSize.Value,
                        totalRecords,
                        "Assigned Leave list fetched successfully");
                }


                var response = ApiResponse<List<allAssignedLeave>>.SuccessResponse(
                    list,
                    "Assigned Leave list fetched successfully");

                response.TotalRecords = totalRecords;

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponse<List<allAssignedLeave>>.FailureResponse(
                    ex.Message,
                    500,
                    "Assigned_Leave_FETCH_ERROR");
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        public async Task<ApiResponse<List<applyLeave>>> GetAllLeaveRequests(string? searchTerm, int? pageNumber, int? pageSize)
        {
            List<applyLeave> list = new();
            int totalRecords = 0;

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_leave", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "getallleaverequests");

                    cmd.Parameters.AddWithValue("@searchTerm",
                        string.IsNullOrWhiteSpace(searchTerm) ? DBNull.Value : searchTerm);

                    cmd.Parameters.AddWithValue("@pageNumber",
                        pageNumber.HasValue ? pageNumber.Value : DBNull.Value);

                    cmd.Parameters.AddWithValue("@pageSize",
                        pageSize.HasValue ? pageSize.Value : DBNull.Value);

                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await sdr.ReadAsync())
                        {

                            if (totalRecords == 0)
                                totalRecords = Convert.ToInt32(sdr["TotalRecords"]);

                            list.Add(new applyLeave
                            {
                                id = Convert.ToInt32(sdr["id"]),
                                leaveCount = sdr["leaveCount"] != DBNull.Value ? Convert.ToInt32(sdr["leaveCount"]) : null,
                                leaveName = sdr["leaveName"] != DBNull.Value ? sdr["leaveName"].ToString() : null,
                                description = sdr["description"] != DBNull.Value ? sdr["description"].ToString() : null,
                                empName = sdr["empName"].ToString(),
                                fromDate = Convert.ToDateTime(sdr["fromDate"]),
                                toDate = Convert.ToDateTime(sdr["toDate"]),
                                applieddate = Convert.ToDateTime(sdr["appliedDate"]),
                                status = Convert.ToInt32(sdr["adminStatus"]) == 0 ? "Pending" : Convert.ToInt32(sdr["adminStatus"]) == 1 ? "Approved" : Convert.ToInt32(sdr["adminStatus"]) == 2 ? "Unapproved" : "",
                                fileName = sdr["uploadedDocument"] != DBNull.Value ? sdr["uploadedDocument"].ToString() : "",
                                statuscode = Convert.ToInt32(sdr["adminStatus"]),
                                reason = sdr["adminDescription"] != DBNull.Value ? sdr["adminDescription"].ToString():""
                            });
                        }
                    }
                }


                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    return ApiResponse<List<applyLeave>>.PagedResponse(
                        list,
                        pageNumber.Value,
                        pageSize.Value,
                        totalRecords,
                        "Assigned Leave list fetched successfully");
                }


                var response = ApiResponse<List<applyLeave>>.SuccessResponse(
                    list,
                    "Assigned Leave list fetched successfully");

                response.TotalRecords = totalRecords;

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponse<List<applyLeave>>.FailureResponse(
                    ex.Message,
                    500,
                    "Assigned_Leave_FETCH_ERROR");
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        public async Task<bool> UpdateLeaveRequest(int id,int status,string reason = null)
        {
            try
            {
                await _conn.OpenAsync();
                using(SqlCommand cmd = new SqlCommand("sp_leave", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@adminStatus", status);
                    cmd.Parameters.AddWithValue("@description", reason);
                    cmd.Parameters.AddWithValue("@action", "updateLeaveRequest");
                    int res = await cmd.ExecuteNonQueryAsync();
                    return res > 0;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        public async Task<ApiResponse<List<Leave>>> getAllLeaveById(int id)
        {
            List<Leave> list = new();

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_leave", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectLeaveById");
                    cmd.Parameters.AddWithValue("@id", id);

                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await sdr.ReadAsync())
                        {
                            list.Add(new Leave
                            {
                                Id = Convert.ToInt32(sdr["id"]),
                                leaveName = sdr["leaveType"] != DBNull.Value ? sdr["leaveType"].ToString() : null,
                                description = sdr["description"] != DBNull.Value ? sdr["description"].ToString() : null,
                            });
                        }
                    }
                }

                if (!list.Any())
                {
                    return ApiResponse<List<Leave>>.FailureResponse(
                        "Leave not found",
                        404,
                        "Leave_NOT_FOUND"
                    );
                }


                return ApiResponse<List<Leave>>.SuccessResponse(
                    list,
                    "Leave fetched successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<List<Leave>>.FailureResponse(
                    ex.Message,
                    500,
                    "Leave_FETCH_ERROR"
                );
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        public async Task<bool> deleteLeaveById(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_leave", _conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id",id);
                cmd.Parameters.AddWithValue("@action ","deleteLeaveById");
                if (_conn.State != ConnectionState.Open)
                    await _conn.OpenAsync();
                int row = await cmd.ExecuteNonQueryAsync();
                return row > 0;
            }
            catch(Exception ex)
            {
                throw;
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        public async Task<bool> InsertAssignLeaveAsync(AssignLeave model)
        {
            // 🔹 Business validation
            if (model.EmployeeId <= 0 ||
                string.IsNullOrWhiteSpace(model.Leave) ||
                string.IsNullOrWhiteSpace(model.LeaveNo))
            {
                throw new ArgumentException(
                    "EmployeeId, Leave and LeaveNo are required");
            }

            var leaveTypes = model.Leave
                .Split(',', StringSplitOptions.RemoveEmptyEntries);

            var leaveNos = model.LeaveNo
                .Split(',', StringSplitOptions.RemoveEmptyEntries);

            if (leaveTypes.Length != leaveNos.Length)
            {
                throw new ArgumentException(
                    "Leave count and Leave type mismatch");
            }

            int rowsAffected = 0;

            try
            {
                if (_conn.State != ConnectionState.Open)
                    await _conn.OpenAsync();

                // 🔹 DB logic
                for (int i = 0; i < leaveTypes.Length; i++)
                {
                    using SqlCommand cmd =
                        new SqlCommand("sp_assignLeave", _conn);

                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@empId", model.EmployeeId);
                    cmd.Parameters.AddWithValue("@leaveType", leaveTypes[i].Trim());
                    cmd.Parameters.AddWithValue("@noOfLeave", leaveNos[i].Trim());
                    cmd.Parameters.AddWithValue("@action", "assignLeaveToEmp");

                    rowsAffected += await cmd.ExecuteNonQueryAsync();
                }
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }

            return rowsAffected > 0;
        }


        public async Task<ApiResponse<List<showLeave>>> getAllAssignedLeaveById(int empId)
        {
            List<showLeave> list = new List<showLeave>();
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_assignLeave", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "getEmpAssignedLeaveById");
                    cmd.Parameters.AddWithValue("@empId", empId);

                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await sdr.ReadAsync())
                        {
                            list.Add(new showLeave
                            {
                                id = Convert.ToInt32(sdr["id"]),
                                leaveType = sdr["leaveType"] != DBNull.Value
                                                    ? sdr["leaveType"].ToString()
                                                    : "",
                                noOfLeave = sdr["noOfLeave"] != DBNull.Value ? Convert.ToInt32(sdr["noOfLeave"]):(int?)null
                            });
                        }
                    }
                }

                if (list.Any())
                {
                    return ApiResponse<List<showLeave>>.SuccessResponse(
                        list,
                        "Assigned leave fetched successfully."
                    );
                }
                else
                {
                    return ApiResponse<List<showLeave>>.FailureResponse(
                        "No assigned leave found.",
                        404
                    );
                }
            }
            catch (Exception ex)
            {
                return ApiResponse<List<showLeave>>.FailureResponse(
                    "An error occurred while fetching assigned leave.",
                    500,
                    errorCode: "EXCEPTION",
                    validationErrors: null
                );
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        #endregion


        #region attendance

        public async Task<int> UploadAttendance(IFormFile file,int selectedMonth, int currentYear)
        {
            int count = 0;
            var attendanceList = _upload.ReadExcelFile(file, selectedMonth, currentYear);

            foreach (var item in attendanceList)
            {
                await SaveAttendance(item);
                count++;
            }

            return count;
        }

        private async Task<int> SaveAttendance(Attendance data)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_attendance", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@empCode", data.EmpCode);
                    cmd.Parameters.AddWithValue("@empName", data.EmpName);
                    cmd.Parameters.AddWithValue("@attendanceDate", data.AttendanceDate);
                    cmd.Parameters.AddWithValue("@status", data.Status);
                    cmd.Parameters.AddWithValue("@inTime", data.InTime == TimeSpan.Zero ? DBNull.Value : (object)data.InTime);
                    cmd.Parameters.AddWithValue("@outTime", data.OutTime == TimeSpan.Zero ? DBNull.Value : (object)data.OutTime);
                    cmd.Parameters.AddWithValue("@totalHours", data.TotalHours ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@day", data.Day);
                    cmd.Parameters.AddWithValue("@month", data.Month);
                    cmd.Parameters.AddWithValue("@year", data.Year);
                    cmd.Parameters.AddWithValue("@action", "uploadAttendance");

                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    await cmd.ExecuteNonQueryAsync();
                }

                return 1;
            }
            catch (Exception ex)
            {
                return 0; // indicate failure for this row
            }
        }

        public async Task<ApiResponse<List<Attendance>>> EmpAttendanceDataByEmpCode(string empCode, int month, int year)
        {
            List<Attendance> list = new List<Attendance>();
            int totalRecords = 0;
            try
            {
                using SqlCommand cmd = new SqlCommand("sp_attendance", _conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "getEmpAttendanceByEmpCode");
                cmd.Parameters.AddWithValue("@empCode", empCode);
                cmd.Parameters.AddWithValue("@month", month);
                cmd.Parameters.AddWithValue("@year", year);

                if (_conn.State != ConnectionState.Open)
                    await _conn.OpenAsync();

                using SqlDataReader sdr = await cmd.ExecuteReaderAsync();

                if (sdr.HasRows)
                {
                    while (await sdr.ReadAsync())
                    {
                        if (sdr["TotalRecords"] != DBNull.Value)
                            totalRecords = Convert.ToInt32(sdr["TotalRecords"]);

                        list.Add(new Attendance
                        {
                            EmpCode = sdr["empCode"] != DBNull.Value ? sdr["empCode"].ToString() : null,
                            EmpName = sdr["empName"] != DBNull.Value ? sdr["empName"].ToString() : null,
                            AttendanceDate = sdr["attendanceDate"] != DBNull.Value ? (DateTime?)sdr["attendanceDate"] : null,
                            Status = sdr["status"] != DBNull.Value ? sdr["status"].ToString() : null,
                            InTime = ParseTimeSpan(sdr["inTime"]),
                            OutTime = ParseTimeSpan(sdr["outTime"]),
                            TotalHours = sdr["totalHours"] != DBNull.Value ? Convert.ToDecimal(sdr["totalHours"]) : (decimal?)null,
                            Day = sdr["day"] != DBNull.Value ? Convert.ToInt32(sdr["day"]) : (int?)null,
                            Month = sdr["month"] != DBNull.Value ? Convert.ToInt32(sdr["month"]) : (int?)null,
                            Year = sdr["year"] != DBNull.Value ? Convert.ToInt32(sdr["year"]) : (int?)null
                        });
                    }
                }

                var response = ApiResponse<List<Attendance>>.SuccessResponse(list, "Employee attendance fetched successfully");
                response.TotalRecords = totalRecords;
                return response;
            }
            catch (Exception ex)
            {
                return ApiResponse<List<Attendance>>.FailureResponse(
                    "Failed to fetch employee attendance",
                    500,
                    errorCode: "EMP_ATTENDANCE_ERROR",
                    validationErrors: null
                );
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }


        private TimeSpan? ParseTimeSpan(object dbValue)
        {
            if (dbValue == null || dbValue == DBNull.Value)
                return null;

            string text = dbValue.ToString().Replace(".", ":").Trim();

            if (TimeSpan.TryParse(text, out var ts))
                return ts;

            if (int.TryParse(text, out int hhmm))
            {
                int hours = hhmm / 100;
                int minutes = hhmm % 100;
                if (hours >= 0 && hours < 24 && minutes >= 0 && minutes < 60)
                    return new TimeSpan(hours, minutes, 0);
            }

            return null;
        }


        public async Task<ApiResponse<List<EmpWorkingDetails>>> EmpWorkingDetailsByempCode(
        int empCode, int month, int year)
        {
            List<EmpWorkingDetails> list = new();

            try
            {
                using SqlCommand cmd = new SqlCommand("sp_attendance", _conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.Add("@empCode", SqlDbType.Int).Value = empCode;
                cmd.Parameters.Add("@month", SqlDbType.Int).Value = month;
                cmd.Parameters.Add("@year", SqlDbType.Int).Value = year;
                cmd.Parameters.AddWithValue("@action", "EmpWorkDetailAttendanceList");

                if (_conn.State != ConnectionState.Open)
                    await _conn.OpenAsync();

                 using SqlDataReader sdr = await cmd.ExecuteReaderAsync();

                while (await sdr.ReadAsync())
                {
                    list.Add(new EmpWorkingDetails
                    {
                        empCode = sdr["empCode"].ToString(),
                        empName = sdr["empName"].ToString(),
                        totalWorkingDays = Convert.ToInt32(sdr["totalWorkingDays"]),
                        presentDays = Convert.ToInt32(sdr["presentDays"]),
                        absentDays = Convert.ToInt32(sdr["absentDays"]),
                        lateEntries = Convert.ToInt32(sdr["lateEntries"]),
                        halfDays = Convert.ToInt32(sdr["halfDays"]),
                        totalWorkingHours = Convert.ToDecimal(sdr["totalWorkingHours"])
                    });
                }

                return ApiResponse<List<EmpWorkingDetails>>
                    .SuccessResponse(list, "Employee working details fetched successfully");
            }
            catch (Exception)
            {
                return ApiResponse<List<EmpWorkingDetails>>
                    .FailureResponse("Failed to fetch employee working details", 500);
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }


        #endregion


        #region project
        public async Task<bool> AddProject(project data)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_addAndAssignProject", _conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", data.id);
                cmd.Parameters.AddWithValue("@category", data.category);
                cmd.Parameters.AddWithValue("@projectTitle", data.projectTitle);
                cmd.Parameters.AddWithValue("@description", data.description);
                cmd.Parameters.AddWithValue("@web", data.web);
                cmd.Parameters.AddWithValue("@app", data.app);
                cmd.Parameters.AddWithValue("@androidApp", data.androidApp);
                cmd.Parameters.AddWithValue("@IOSApp", data.IOSApp);
                cmd.Parameters.AddWithValue("@appTechnology", data.appTechnology);
                cmd.Parameters.AddWithValue("@appEmpId", data.appEmpId);
                cmd.Parameters.AddWithValue("@webTechnology", data.webTechnology);
                cmd.Parameters.AddWithValue("@webEmpId", data.webEmpId);
                cmd.Parameters.AddWithValue("@startDate", data.startDate);
                cmd.Parameters.AddWithValue("@assignDate", data.assignDate);
                cmd.Parameters.AddWithValue("@endDate", data.endDate);
                cmd.Parameters.AddWithValue("@completionDate", data.completionDate);
                cmd.Parameters.AddWithValue("@sopDocument", data.sopDocumentPath);
                cmd.Parameters.AddWithValue("@technicalDocument", data.technicalDocumentPath);
                cmd.Parameters.AddWithValue("@SEO", data.SEO);
                cmd.Parameters.AddWithValue("@SMO", data.SMO);
                cmd.Parameters.AddWithValue("@paidAds", data.paidAds);
                cmd.Parameters.AddWithValue("@GMB", data.GMB);
                cmd.Parameters.AddWithValue("@action", data.id > 0 ? "updateProject" : "insertProject");

                SqlParameter resultParam = new SqlParameter("@result", SqlDbType.Int)
                {
                    Direction = ParameterDirection.Output
                };
                cmd.Parameters.Add(resultParam);

                await _conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();


                int result = resultParam.Value != DBNull.Value
                    ? Convert.ToInt32(resultParam.Value)
                    : 0;

                return result == 1;

            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        public async Task<ApiResponse<List<project>>> GetAllProject(string? SearchTerm,int? pageNumber,int?pageSize, int? userId = null,string? status=null)
        {
            List<project> list = new List<project>();
            int totalRecords = 0;
            try
            {
                SqlCommand cmd = new SqlCommand("sp_addAndAssignProject", _conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action" ,"getAllProjectDetail");
                cmd.Parameters.AddWithValue("@searchTerm",string.IsNullOrWhiteSpace(SearchTerm)?DBNull.Value:SearchTerm);
                cmd.Parameters.AddWithValue("@pageNumber",pageNumber.HasValue?pageNumber.Value:DBNull.Value);
                cmd.Parameters.AddWithValue("@pageSize",pageSize.HasValue?pageSize.Value:DBNull.Value);
                cmd.Parameters.AddWithValue("@id",userId.HasValue?userId.Value:DBNull.Value);
                cmd.Parameters.AddWithValue("@status", string.IsNullOrWhiteSpace(status)?(object)DBNull.Value:status);
                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();

                using(SqlDataReader sdr=await cmd.ExecuteReaderAsync())
                {
                    if(sdr.HasRows)
                    {
                        while(await sdr.ReadAsync())
                        {
                            if (totalRecords == 0)
                                totalRecords = Convert.ToInt32(sdr["TotalRecords"]);
                            list.Add(new project
                            {
                                id = Convert.ToInt32(sdr["id"]),
                                category = sdr["category"] != DBNull.Value ? sdr["category"].ToString():null,
                                projectTitle = sdr["projectTitle"] != DBNull.Value ? sdr["projectTitle"].ToString():null,
                                description = sdr["description"] != DBNull.Value ? sdr["description"].ToString():null,
                                //web = sdr["web"] != DBNull.Value ? sdr["web"].ToString():null,
                                //app = sdr["app"] != DBNull.Value ? sdr["app"].ToString():null,
                                //androidApp = sdr["androidApp"] != DBNull.Value ? sdr["androidApp"].ToString():null,
                                //IOSApp = sdr["IOSApp"] != DBNull.Value ? sdr["IOSApp"].ToString():null,

                                web = sdr["web"] != DBNull.Value && Convert.ToBoolean(sdr["web"]),
                                app = sdr["app"] != DBNull.Value && Convert.ToBoolean(sdr["app"]),
                                androidApp = sdr["androidApp"] != DBNull.Value && Convert.ToBoolean(sdr["androidApp"]),
                                IOSApp = sdr["IOSApp"] != DBNull.Value && Convert.ToBoolean(sdr["IOSApp"]),

                                appTechnology = sdr["appTechnology"] != DBNull.Value ? Convert.ToInt32(sdr["appTechnology"]):null,
                                appTechnologyName = sdr["AppTechnology"] != DBNull.Value ? sdr["AppTechnology"].ToString():null,
                                webTechnology = sdr["webTechnology"] != DBNull.Value ? Convert.ToInt32(sdr["webTechnology"]):null,
                                webTechnologyName = sdr["webTechnology"] != DBNull.Value ? sdr["webTechnology"].ToString():null,
                                appEmpId = sdr["appEmpId"] != DBNull.Value ? Convert.ToInt32(sdr["appEmpId"]):null,
                                appEmpName = sdr["AppTeamLead"] != DBNull.Value ?sdr["AppTeamLead"].ToString():null,
                                webEmpId = sdr["webEmpId"] != DBNull.Value ? Convert.ToInt32(sdr["webEmpId"]):null,
                                webEmpName = sdr["WebTeamLead"] != DBNull.Value ? sdr["WebTeamLead"].ToString():null,
                                startDate = sdr["startDate"] != DBNull.Value ?Convert.ToDateTime(sdr["startDate"]):DateTime.MinValue,
                                assignDate = sdr["assignDate"] != DBNull.Value ? Convert.ToDateTime(sdr["assignDate"]):DateTime.MinValue,
                                endDate = sdr["endDate"] != DBNull.Value ? Convert.ToDateTime(sdr["endDate"]):DateTime.MinValue,
                                completionDate = sdr["completionDate"] != DBNull.Value ? Convert.ToDateTime(sdr["completionDate"]):DateTime.MinValue,
                                SEO = sdr["SEO"] != DBNull.Value ? sdr["SEO"].ToString():null,
                                SMO = sdr["SMO"] != DBNull.Value ? sdr["SMO"].ToString():null,
                                paidAds = sdr["paidAds"] != DBNull.Value ? sdr["paidAds"].ToString():null,
                                GMB = sdr["GMB"] != DBNull.Value ? sdr["GMB"].ToString():null,
                                sopDocumentPath = sdr["sopDocument"] != DBNull.Value ? sdr["sopDocument"].ToString():null,
                                technicalDocumentPath = sdr["technicalDocument"] != DBNull.Value ? sdr["technicalDocument"].ToString():null,
                                delayedDays = sdr["DelayedDays"] != DBNull.Value ? Convert.ToInt32(sdr["DelayedDays"]) : null,
                                projectStatus = sdr["projectStatus"] != DBNull.Value ? sdr["projectStatus"].ToString():null,
                                appProjectMembers = sdr["AppMembersJson"] != DBNull.Value
                                ? JsonSerializer.Deserialize<List<appProjectMember>>(sdr["AppMembersJson"].ToString())
                                : new List<appProjectMember>(),
                                webProjectMembers = sdr["WebMembersJson"] != DBNull.Value
                                    ? JsonSerializer.Deserialize<List<webProjectMember>>(sdr["WebMembersJson"].ToString())
                                    : new List<webProjectMember>()
                            });
                        }
                    }
                }

                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    return ApiResponse<List<project>>.PagedResponse(
                        list,
                        pageNumber.Value,
                        pageSize.Value,
                        totalRecords,
                        "Project data list fetched successfully");
                }

                var response = ApiResponse<List<project>>.SuccessResponse(
                    list,
                    "Project data list fetched successfully");

                response.TotalRecords = totalRecords;

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponse<List<project>>.FailureResponse(
                    "Failed to fetch Project data",
                    500,
                    "PROJECT_FETCH_ERROR");
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }


        public async Task<ApiResponse<List<project>>> GetAllProjectById(int id)
        {
            List<project> list = new List<project>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_addAndAssignProject", _conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "getProjectDetailById");
                cmd.Parameters.AddWithValue("@id", id);
            
                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();

                using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                {
                    if (sdr.HasRows)
                    {
                        while (await sdr.ReadAsync())
                        {
                            
                            list.Add(new project
                            {
                                id = Convert.ToInt32(sdr["id"]),
                                category = sdr["category"] != DBNull.Value ? sdr["category"].ToString() : null,
                                projectTitle = sdr["projectTitle"] != DBNull.Value ? sdr["projectTitle"].ToString() : null,
                                description = sdr["description"] != DBNull.Value ? sdr["description"].ToString() : null,
                                web = sdr["web"] != DBNull.Value && Convert.ToBoolean(sdr["web"]),
                                app = sdr["app"] != DBNull.Value && Convert.ToBoolean(sdr["app"]),
                                androidApp = sdr["androidApp"] != DBNull.Value && Convert.ToBoolean(sdr["androidApp"]),
                                IOSApp = sdr["IOSApp"] != DBNull.Value && Convert.ToBoolean(sdr["IOSApp"]),

                                appTechnology = sdr["appTechnology"] != DBNull.Value ? Convert.ToInt32(sdr["appTechnology"]) : null,
                                appTechnologyName = sdr["AppTechnology"] != DBNull.Value ? sdr["AppTechnology"].ToString() : null,
                                webTechnology = sdr["webTechnology"] != DBNull.Value ? Convert.ToInt32(sdr["webTechnology"]) : null,
                                webTechnologyName = sdr["webTechnology"] != DBNull.Value ? sdr["webTechnology"].ToString() : null,
                                appEmpId = sdr["appEmpId"] != DBNull.Value ? Convert.ToInt32(sdr["appEmpId"]) : null,
                                appEmpName = sdr["appEmp"] != DBNull.Value ? sdr["appEmp"].ToString() : null,
                                webEmpId = sdr["webEmpId"] != DBNull.Value ? Convert.ToInt32(sdr["webEmpId"]) : null,
                                webEmpName = sdr["webEmp"] != DBNull.Value ? sdr["webEmp"].ToString() : null,
                                startDate = sdr["startDate"] != DBNull.Value ? Convert.ToDateTime(sdr["startDate"]) : DateTime.MinValue,
                                assignDate = sdr["assignDate"] != DBNull.Value ? Convert.ToDateTime(sdr["assignDate"]) : DateTime.MinValue,
                                endDate = sdr["endDate"] != DBNull.Value ? Convert.ToDateTime(sdr["endDate"]) : DateTime.MinValue,
                                completionDate = sdr["completionDate"] != DBNull.Value ? Convert.ToDateTime(sdr["completionDate"]) : DateTime.MinValue,
                                SEO = sdr["SEO"] != DBNull.Value ? sdr["SEO"].ToString() : null,
                                SMO = sdr["SMO"] != DBNull.Value ? sdr["SMO"].ToString() : null,
                                paidAds = sdr["paidAds"] != DBNull.Value ? sdr["paidAds"].ToString() : null,
                                GMB = sdr["GMB"] != DBNull.Value ? sdr["GMB"].ToString() : null,
                                sopDocumentPath = sdr["sopDocument"] != DBNull.Value ? sdr["sopDocument"].ToString() : null,
                                technicalDocumentPath = sdr["technicalDocument"] != DBNull.Value ? sdr["technicalDocument"].ToString() : null,
                            });
                        }
                    }
                }

                if (!list.Any())
                    return ApiResponse<List<project>>.FailureResponse(
                        "No project found",
                        404,
                        "PROJECT_NOT_FOUND");

                return ApiResponse<List<project>>.SuccessResponse(
                    list,
                    "Project data list fetched successfully");
            }
            catch (Exception)
            {
                return ApiResponse<List<project>>.FailureResponse(
                    "Failed to fetch Project data",
                    500,
                    "PROJECT_FETCH_ERROR");
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        public async Task<ApiResponse<List<project>>> GetEmpProjectDetailByEmpId(int empId)
        {
            List<project> list = new List<project>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_addAndAssignProject", _conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "getEmpProjectDetailByEmpId");
                cmd.Parameters.AddWithValue("@id", empId);

                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();

                using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                {
                    if (sdr.HasRows)
                    {
                        while (await sdr.ReadAsync())
                        {

                            list.Add(new project
                            {
                                id = Convert.ToInt32(sdr["id"]),
                                category = sdr["category"] != DBNull.Value ? sdr["category"].ToString() : null,
                                projectTitle = sdr["projectTitle"] != DBNull.Value ? sdr["projectTitle"].ToString() : null,
                                description = sdr["description"] != DBNull.Value ? sdr["description"].ToString() : null,
                                web = sdr["web"] != DBNull.Value && Convert.ToBoolean(sdr["web"]),
                                app = sdr["app"] != DBNull.Value && Convert.ToBoolean(sdr["app"]),
                                androidApp = sdr["androidApp"] != DBNull.Value && Convert.ToBoolean(sdr["androidApp"]),
                                IOSApp = sdr["IOSApp"] != DBNull.Value && Convert.ToBoolean(sdr["IOSApp"]),

                                appTechnology = sdr["appTechnology"] != DBNull.Value ? Convert.ToInt32(sdr["appTechnology"]) : null,
                                appTechnologyName = sdr["AppTechnology"] != DBNull.Value ? sdr["AppTechnology"].ToString() : null,
                                webTechnology = sdr["webTechnology"] != DBNull.Value ? Convert.ToInt32(sdr["webTechnology"]) : null,
                                webTechnologyName = sdr["WebTechnologyName"] != DBNull.Value ? sdr["WebTechnologyName"].ToString() : null,
                                appEmpId = sdr["appEmpId"] != DBNull.Value ? Convert.ToInt32(sdr["appEmpId"]) : null,
                                appEmpName = sdr["appEmp"] != DBNull.Value ? sdr["appEmp"].ToString() : null,
                                webEmpId = sdr["webEmpId"] != DBNull.Value ? Convert.ToInt32(sdr["webEmpId"]) : null,
                                webEmpName = sdr["webEmp"] != DBNull.Value ? sdr["webEmp"].ToString() : null,
                                startDate = sdr["startDate"] != DBNull.Value ? Convert.ToDateTime(sdr["startDate"]) : DateTime.MinValue,
                                assignDate = sdr["assignDate"] != DBNull.Value ? Convert.ToDateTime(sdr["assignDate"]) : DateTime.MinValue,
                                endDate = sdr["endDate"] != DBNull.Value ? Convert.ToDateTime(sdr["endDate"]) : DateTime.MinValue,
                                completionDate = sdr["completionDate"] != DBNull.Value ? Convert.ToDateTime(sdr["completionDate"]) : DateTime.MinValue,
                                SEO = sdr["SEO"] != DBNull.Value ? sdr["SEO"].ToString() : null,
                                SMO = sdr["SMO"] != DBNull.Value ? sdr["SMO"].ToString() : null,
                                paidAds = sdr["paidAds"] != DBNull.Value ? sdr["paidAds"].ToString() : null,
                                GMB = sdr["GMB"] != DBNull.Value ? sdr["GMB"].ToString() : null,
                                sopDocumentPath = sdr["sopDocument"] != DBNull.Value ? sdr["sopDocument"].ToString() : null,
                                technicalDocumentPath = sdr["technicalDocument"] != DBNull.Value ? sdr["technicalDocument"].ToString() : null,
                            });
                        }
                    }
                }

                if (!list.Any())
                    return ApiResponse<List<project>>.FailureResponse(
                        "No project found",
                        404,
                        "PROJECT_NOT_FOUND");

                return ApiResponse<List<project>>.SuccessResponse(
                    list,
                    "Project data list fetched successfully");
            }
            catch (Exception)
            {
                return ApiResponse<List<project>>.FailureResponse(
                    "Failed to fetch Project data",
                    500,
                    "PROJECT_FETCH_ERROR");
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        public async Task<bool> deleteProjectById(int id)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_addAndAssignProject", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "deletedProjectById");
                    cmd.Parameters.AddWithValue("@id", id);

                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    _conn.CloseAsync();
            }
        }



        #endregion


        #region task 

        public async Task<bool> insertTask(Taskassign data)
        {
            try
            {
                using SqlCommand cmd = new SqlCommand("sp_assignTask", _conn);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@id", data.id);
                cmd.Parameters.AddWithValue("@empId", data.empId);
                cmd.Parameters.AddWithValue("@title", data.title);
                cmd.Parameters.AddWithValue("@description", data.description);
                cmd.Parameters.AddWithValue("@completedDate", data.CompletedDate);
                cmd.Parameters.AddWithValue("@assignedBy", data.assignedBy);
                cmd.Parameters.AddWithValue("@autoReassign", data.autoReassign);
                cmd.Parameters.AddWithValue("@adminResponse", data.adminResponse);
                cmd.Parameters.AddWithValue("@timePeriodDays", data.timePeriodDay);
                cmd.Parameters.AddWithValue("@document1", data.document1Path ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@document2", data.document2Path ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@action",data.id>0? "update":"insert");

                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();

                int rows = await cmd.ExecuteNonQueryAsync();
                return rows > 0;
            }
            catch
            {
                throw;
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }


        public async Task<ApiResponse<List<TaskAssignDto>>> getAllAssignTask(string? searchTerm, int? pageNumber, int? pageSize, int? empId = null,string? statusTerm=null)
        {
            List<TaskAssignDto> list = new();
            int totalRecords = 0;
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_assignTask", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectAll");
                    cmd.Parameters.AddWithValue("@empId", empId.HasValue ? empId.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@searchTerm", string.IsNullOrWhiteSpace(searchTerm) ? DBNull.Value :searchTerm);
                    cmd.Parameters.AddWithValue("@statusTerm ", string.IsNullOrWhiteSpace(statusTerm) ? DBNull.Value : statusTerm);
                    cmd.Parameters.AddWithValue("@pageNumber", pageNumber.HasValue ? pageNumber.Value : DBNull.Value);
                    cmd.Parameters.AddWithValue("@pageSize",pageSize.HasValue ? pageSize.Value : DBNull.Value);
                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();
                    using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await sdr.ReadAsync())
                        {
                            if (totalRecords == 0)
                                totalRecords = Convert.ToInt32(sdr["TotalRecords"]);
                            list.Add(new TaskAssignDto
                            {
                                id = Convert.ToInt32(sdr["id"]),
                                empId = sdr["empId"] != DBNull.Value ? Convert.ToInt32(sdr["empId"]):null,
                                title = sdr["title"] != DBNull.Value ? sdr["title"].ToString() : null,
                                description = sdr["description"] != DBNull.Value ? sdr["description"].ToString() : null,
                                CompletedDate = sdr["CompletedDate"] != DBNull.Value ? Convert.ToDateTime(sdr["CompletedDate"]) : null,
                                document1Path = sdr["document1"] != DBNull.Value ? sdr["document1"].ToString() : null,
                                document2Path = sdr["document2"] != DBNull.Value ? sdr["document2"].ToString() : null,
                                empName = sdr["empName"] != DBNull.Value ? sdr["empName"].ToString() : null,
                                assignedDate= sdr["createdAt"] != DBNull.Value ? Convert.ToDateTime(sdr["CompletedDate"]) : null,
                                taskStatus = sdr["taskStatusEmp"] != DBNull.Value ? sdr["taskStatusEmp"].ToString() : null,
                                adminTaskStatus = sdr["AdminTaskStatus"] != DBNull.Value ? sdr["AdminTaskStatus"].ToString() : null,
                                assignedByName = sdr["roleName"] != DBNull.Value ? sdr["roleName"].ToString()! : "",
                                autoReassign = sdr["autoReassign"] != DBNull.Value ? Convert.ToBoolean(sdr["autoReassign"]):null,
                                timePeriodDays = sdr["timePeriodDays"] != DBNull.Value ? Convert.ToInt32(sdr["timePeriodDays"]):null

                            });
                        }
                    }
                }
                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    return ApiResponse<List<TaskAssignDto>>.PagedResponse(
                        list,
                        pageNumber.Value,
                        pageSize.Value,
                        totalRecords,
                        "Assign Task list fetched successfully");
                }
                var response = ApiResponse<List<TaskAssignDto>>.SuccessResponse(list,"Assign Task list fetched successfully");
                response.TotalRecords = totalRecords;
                return response;
            }
            catch (Exception ex)
            {
                return ApiResponse<List<TaskAssignDto>>.FailureResponse( ex.Message, 500,"ASSIGN_TASK_FETCH_ERROR");
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }


        public async Task<ApiResponse<List<Taskassign>>> getAllAssignTaskById(int id)
        {
            List<Taskassign> list = new();

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_assignTask", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "selectAllById");
                    cmd.Parameters.AddWithValue("@id", id);

                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await sdr.ReadAsync())
                        {
                        

                            list.Add(new Taskassign
                            {
                                id = Convert.ToInt32(sdr["id"]),

                                empId = sdr["empId"] != DBNull.Value
                                        ? Convert.ToInt32(sdr["empId"])
                                        : (int?)null,

                                title = sdr["title"]?.ToString(),
                                description = sdr["description"]?.ToString(),

                                CompletedDate = sdr["CompletedDate"] != DBNull.Value
                                                ? Convert.ToDateTime(sdr["CompletedDate"])
                                                : (DateTime?)null,

                                document1Path = sdr["document1"]?.ToString(),
                                document2Path = sdr["document2"]?.ToString(),
                                empName = sdr["empName"]?.ToString()
                            });
                        }
                    }
                }

                return ApiResponse<List<Taskassign>>.SuccessResponse(
                    list,
                    "Assign Task list fetched successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<List<Taskassign>>.FailureResponse(
                    ex.Message,
                    500,
                    "ASSIGN_TASK_FETCH_ERROR"
                );
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        public async Task<bool> deleteTaskById(int id)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_assignTask", _conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@action ", "deleteById");
                if (_conn.State != ConnectionState.Open)
                    await _conn.OpenAsync();
                int row = await cmd.ExecuteNonQueryAsync();
                return row > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }
        #endregion


        #region admin dashboard
        public async Task<ApiResponse<List<AdminDashboardCountDto>>> adminDashboardCount()
        {
            List<AdminDashboardCountDto> list = new List<AdminDashboardCountDto>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_empDashboard", _conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "adminDashboardCount");
                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();

                using(SqlDataReader sdr=await cmd.ExecuteReaderAsync())
                {
                    if(sdr.HasRows)
                    {
                        while(await sdr.ReadAsync())
                        {
                            list.Add(new AdminDashboardCountDto
                            {
                                TotalEmployees = sdr["TotalEmployees"] != DBNull.Value ? Convert.ToInt32(sdr["TotalEmployees"]):null,
                                AbsentEmployee = sdr["AbsentEmployee"] != DBNull.Value ? Convert.ToInt32(sdr["AbsentEmployee"]):null,
                                TotalProjects = sdr["TotalProjects"] != DBNull.Value ? Convert.ToInt32(sdr["TotalProjects"]):null,
                                OngoingProjects = sdr["OngoingProjects"] != DBNull.Value ? Convert.ToInt32(sdr["OngoingProjects"]):null,
                                TotalTasks = sdr["TotalTasks"] != DBNull.Value ? Convert.ToInt32(sdr["TotalTasks"]):null,
                                CompletedTask = sdr["CompletedTask"] != DBNull.Value ? Convert.ToInt32(sdr["CompletedTask"]):null,
                                LeaveRequest = sdr["LeaveRequest"] != DBNull.Value ? Convert.ToInt32(sdr["LeaveRequest"]):null,
                                UpcomingLeaves = sdr["UpcomingLeaves"] != DBNull.Value ? Convert.ToInt32(sdr["UpcomingLeaves"]):null,
                            });
                        }
                    }
                }
                return ApiResponse<List<AdminDashboardCountDto>>.SuccessResponse(
                  list,
                  "Admin dashboard count fetched successfully"
              );
            }
            catch(Exception ex)
            {
                return ApiResponse<List<AdminDashboardCountDto>>.FailureResponse(
                  ex.Message,
                  500,
                  "ASSIGN_TASK_FETCH_ERROR"
              );
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }

        #endregion


        #region assigned project emp list

        public async Task<ApiResponse<List<AssignedProjectEmpListDto>>> assignedProjectEmpList(int projectId, int pmId)
        {
            List<AssignedProjectEmpListDto> list = new List<AssignedProjectEmpListDto>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_addAndAssignProject", _conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "getEmpListForAssignedProject");
                cmd.Parameters.AddWithValue("@projectId", projectId);
                cmd.Parameters.AddWithValue("@pmId", pmId);
                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();

                using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                {
                    if (sdr.HasRows)
                    {
                        while (await sdr.ReadAsync())
                        {
                            list.Add(new AssignedProjectEmpListDto
                            {
                                id = sdr["id"] != DBNull.Value ? Convert.ToInt32(sdr["id"]) : null,
                                projectId = sdr["projectId"] != DBNull.Value ? Convert.ToInt32(sdr["projectId"]) : null,
                                empId = sdr["empId"] != DBNull.Value ? Convert.ToInt32(sdr["empId"]) : null,
                                approveStatus = sdr["approveStatus"] != DBNull.Value ? Convert.ToInt32(sdr["approveStatus"]) : null,
                                empName = sdr["empName"] != DBNull.Value ? sdr["empName"].ToString():null,
                                designationName = sdr["designationName"] != DBNull.Value ? sdr["designationName"].ToString():null
                            });
                        }
                    }
                }
                return ApiResponse<List<AssignedProjectEmpListDto>>.SuccessResponse(
                list,
                "Assigned Project All emp list fetched successfully!!"
            );
            }
            catch (Exception ex)
            {
                return ApiResponse<List<AssignedProjectEmpListDto>>.FailureResponse(
                  ex.Message,
                  500,
                  "ASSIGN_PROJECT_EMP_LIST_FETCH_ERROR"
              );
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }

        }
            
          
        

        #endregion
    }
}
