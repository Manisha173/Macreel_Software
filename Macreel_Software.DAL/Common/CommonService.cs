using System.Data;
using Macreel_Software.Contracts.DTOs;
using Macreel_Software.Models;
using Macreel_Software.Models.Common;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
namespace Macreel_Software.DAL.Common
{
    public class CommonService : ICommonServices
    {
        private readonly IConfiguration _config;
        private readonly SqlConnection _conn;

        public CommonService(IConfiguration config)
        {
            _config = config;
            string connectionString = _config.GetConnectionString("DefaultConnection");
            _conn = new SqlConnection(connectionString);
        }

        #region Register Admin
        public async Task<bool> RegisterAdmin(string Username, string Password)
        {
            using (SqlCommand cmd = new SqlCommand("sp_Login", _conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@Username", Username);
                cmd.Parameters.AddWithValue("@Password", Password);
                cmd.Parameters.AddWithValue("@Action", "adminreg");
                if (_conn.State != ConnectionState.Open)
                    await _conn.OpenAsync();
                int res = await cmd.ExecuteNonQueryAsync();
                return res > 0;
            }
        }
        #endregion

        public async Task<List<state>> GetAllState()
        {
            List<state> list = new List<state>();

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_state", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "getAllState");

                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {

                            state s = new state
                            {
                                stateId = Convert.ToInt32(reader["id"]),
                                stateName = reader["stateName"].ToString()
                            };
                            list.Add(s);
                        }
                    }
                }
            }
            catch (Exception ex)
            {

                throw;
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    _conn.Close();
            }

            return list;
        }

        public async Task<List<city>> getCityById(int stateId)
        {
            List<city> list = new List<city>();
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_state", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "getCityByStateId");
                    cmd.Parameters.AddWithValue("@id", stateId);
                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();
                    using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await sdr.ReadAsync())
                        {
                            list.Add(new city
                            {
                                cityId = Convert.ToInt32(sdr["id"]),
                                stateId = Convert.ToInt32(sdr["stateId"]),
                                stateName = sdr["stateName"].ToString(),
                                cityName = sdr["cityName"].ToString(),
                            });
                        }
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
            return list;
        }

        #region ruleBook

        public async Task<bool> AddUpdateRuleBook(ruleBook data)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("sp_ruleBook", _conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@id", data.id);
                cmd.Parameters.AddWithValue("@ruleBook", data.rule_Book_Path);
                cmd.Parameters.AddWithValue("@action", data.id > 0 ? "updateRuleBook" : "insert");
                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();

                int affectedRow = await cmd.ExecuteNonQueryAsync();
                return affectedRow > 0;
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


        public async Task<ApiResponse<List<ruleBook>>> getAllRulrBook(string? searchTerm, int? pageNumber, int? pageSize)
        {
            List<ruleBook> list = new();
            int totalRecords = 0;

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_ruleBook", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "getAllRuleBook");

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

                            list.Add(new ruleBook
                            {
                                id = Convert.ToInt32(sdr["id"]),
                                rule_Book_Path = sdr["ruleBook"].ToString()
                            });
                        }
                    }
                }


                if (pageNumber.HasValue && pageSize.HasValue)
                {
                    return ApiResponse<List<ruleBook>>.PagedResponse(
                        list,
                        pageNumber.Value,
                        pageSize.Value,
                        totalRecords,
                        "Rulebook list fetched successfully");
                }


                var response = ApiResponse<List<ruleBook>>.SuccessResponse(
                    list,
                    "Rulebook list fetched successfully");

                response.TotalRecords = totalRecords;

                return response;
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ruleBook>>.FailureResponse(
                    ex.Message,
                    500,
                    "Rulebook_FETCH_ERROR");
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }


        public async Task<ApiResponse<List<ruleBook>>> GetRuleBookByIdAsync(int id)
        {
            List<ruleBook> list = new();

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_ruleBook", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "getRuleBookById");
                    cmd.Parameters.AddWithValue("@id", id);

                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await sdr.ReadAsync())
                        {
                            list.Add(new ruleBook
                            {
                                id = Convert.ToInt32(sdr["id"]),
                                rule_Book_Path = sdr["ruleBook"].ToString()
                            });
                        }
                    }
                }

                return ApiResponse<List<ruleBook>>.SuccessResponse(
                    list,
                    "Rulebook fetched successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<ruleBook>>.FailureResponse(
                    ex.Message,
                    500,
                    "RULEBOOK_FETCH_ERROR");
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }


        public async Task<bool> deleteRuleBookById(int id)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_ruleBook", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "deleteRuleBookById");
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

        #region send mail for register
        public async Task<bool> sendMailForReg(sendMailForReg data, string accessId)
        {
            using (SqlCommand cmd = new SqlCommand("sp_sendEmailForEmpReg", _conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@email", data.email);
                cmd.Parameters.AddWithValue("@accessId", accessId);
                cmd.Parameters.AddWithValue("@action", "sendMailForReg");
                if (_conn.State != ConnectionState.Open)
                    await _conn.OpenAsync();
                int res = await cmd.ExecuteNonQueryAsync();
                return res > 0;
            }
        }
        #endregion
        public async Task<ApiResponse<List<SendEmailForRegistrationDto>>> getEmailByAccessByIdForReg(string accessId)
        {
            List<SendEmailForRegistrationDto> data = new List<SendEmailForRegistrationDto>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_sendEmailForEmpReg", _conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "selectEmailByaccessId");
                cmd.Parameters.AddWithValue("@accessId", accessId);
                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();

                using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                {
                    if (sdr.HasRows)
                    {
                        while (await sdr.ReadAsync())
                        {
                            data.Add(new SendEmailForRegistrationDto
                            {
                                id = sdr["id"] != DBNull.Value ? Convert.ToInt32(sdr["id"]) : null,
                                accessId = sdr["accessId"] != DBNull.Value ? sdr["accessId"].ToString() : null,
                                email = sdr["email"] != DBNull.Value ? sdr["email"].ToString() : null,
                                reg_date = sdr["reg_date"] != DBNull.Value ? Convert.ToDateTime(sdr["reg_date"]) : null,
                            });
                        }
                    }
                }
                return ApiResponse<List<SendEmailForRegistrationDto>>.SuccessResponse(
                         data,
                         "Email fetched successfull by accessId");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<SendEmailForRegistrationDto>>.FailureResponse(
                    ex.Message,
                    500,
                    "EMAIL_FETCH_ERROR");
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }
        }


        #endregion

        #region project Employee
        public async Task<bool> InsertProjectEmp(ProjectEmp data, int? userid, string? addedBy)
        {
            bool isInserted = false;

            try
            {
                if (string.IsNullOrWhiteSpace(data.empIds))
                    return false;

                var empIdList = data.empIds
                    .Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(e => e.Trim())
                    .Where(e => !string.IsNullOrEmpty(e))
                    .ToList();

                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();

                foreach (var empId in empIdList)
                {
                    using SqlCommand cmd = new SqlCommand("sp_addAndAssignProject", _conn);
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@action", "insertProjectEmp");
                    cmd.Parameters.AddWithValue("@projectId", data.projectId);
                    cmd.Parameters.AddWithValue("@empId", empId);
                    cmd.Parameters.AddWithValue("@pmId", userid);
                    cmd.Parameters.AddWithValue("@addedBy", addedBy);

                    int rows = await cmd.ExecuteNonQueryAsync();
                    if (rows > 0)
                        isInserted = true;
                }

                return isInserted;
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



        public async Task<ApiResponse<List<AssignedProjectEmpDto>>> AssignedProjectEmpList(int projectId)
        {
            List<AssignedProjectEmpDto> list = new List<AssignedProjectEmpDto>();
            try
            {
                SqlCommand cmd = new SqlCommand("sp_addAndAssignProject", _conn);
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@action", "assignedProjectEmpList");
                cmd.Parameters.AddWithValue("@id", projectId);
                if (_conn.State == ConnectionState.Closed)
                    await _conn.OpenAsync();
                using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                {
                    if (sdr.HasRows)
                    {
                        while (await sdr.ReadAsync())
                        {
                            list.Add(new AssignedProjectEmpDto
                            {
                                Id = sdr["id"] != DBNull.Value ? Convert.ToInt32(sdr["id"]):null,
                                Category = sdr["category"] != DBNull.Value ? sdr["category"].ToString():null,
                                ProjectTitle = sdr["projectTitle"] != DBNull.Value ? sdr["projectTitle"].ToString():null,
                                Description = sdr["description"] != DBNull.Value ? sdr["description"].ToString():null,
                                StartDate = sdr["startDate"] != DBNull.Value ? Convert.ToDateTime(sdr["startDate"]):null,
                                AssignDate = sdr["assignDate"] != DBNull.Value ? Convert.ToDateTime(sdr["assignDate"]):null,
                                EndDate = sdr["endDate"] != DBNull.Value ? Convert.ToDateTime(sdr["endDate"]):null,
                                CompletionDate = sdr["completionDate"] != DBNull.Value ? Convert.ToDateTime(sdr["completionDate"]):null,
                                EmpName = sdr["empName"] != DBNull.Value ? sdr["empName"].ToString():null,
                                Mobile = sdr["mobile"] != DBNull.Value ? sdr["mobile"].ToString():null,
                                Designation = sdr["designationName"] != DBNull.Value ? sdr["designationName"].ToString():null
                            });
                        }
                    }
                }
                return ApiResponse<List<AssignedProjectEmpDto>>.SuccessResponse(
                    list,
                    "Email fetched successfull by accessId");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<AssignedProjectEmpDto>>.FailureResponse(
                    ex.Message,
                    500,
                    "EMAIL_FETCH_ERROR");
            }

        }

        public async Task<bool> UpdateProjectEmpStatusSingle(ProjectEmpStatusItem model, int adminId)
        {
            try
            {
                using (SqlCommand cmd =
                       new SqlCommand("sp_addAndAssignProject", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.AddWithValue("@action", "updateStatusOfProjectEmp");
                    cmd.Parameters.AddWithValue("@projectId", model.ProjectId);
                    cmd.Parameters.AddWithValue("@pmId", model.PmId);
                    cmd.Parameters.AddWithValue("@empId", model.EmpId);
                    cmd.Parameters.AddWithValue("@newEmpId",model.NewEmpId ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@approveStatus", model.Status);
                    cmd.Parameters.AddWithValue("@reason", model.Reason ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@addedBy", adminId);

                    if (_conn.State == ConnectionState.Closed)
                        await _conn.OpenAsync();

                    int rows = await cmd.ExecuteNonQueryAsync();
                    return rows > 0;
                }
            }
            catch (SqlException sqlEx)
            {
                throw; 
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


    }
}

        #endregion

    



