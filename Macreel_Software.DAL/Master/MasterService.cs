using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Macreel_Software.Models.Master;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Macreel_Software.DAL.Master
{
    public class MasterService:IMasterService
    {
        private readonly SqlConnection _conn;

        public MasterService(IConfiguration config)
        {
            _conn = new SqlConnection(
                config.GetConnectionString("DefaultConnection"));
        }

        #region role service
        public async Task<bool> InsertRole(role data)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_role", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", data.id);
                    cmd.Parameters.AddWithValue("@rolename", data.rolename);
                    cmd.Parameters.AddWithValue("@action",data.id>0? "updateRole" : "insert");

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

        public async Task<(List<role> Data, int TotalRecords)> getAllRole( string? searchTerm,int? pageNumber,int? pageSize)
        {
            List<role> list = new();
            int totalRecords = 0;

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_role", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "getAllRole");
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
                            list.Add(new role
                            {
                                id = Convert.ToInt32(sdr["id"]),
                                rolename = sdr["roleName"].ToString()
                            });
                        }

                        // Result set 2 → TotalRecords
                        if (await sdr.NextResultAsync() && await sdr.ReadAsync())
                        {
                            totalRecords = Convert.ToInt32(sdr["TotalRecords"]);
                        }
                    }
                }
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }

            return (list, totalRecords);
        }


        public async Task<List<role>> getAllRoleById(int id)
        {
            List<role> list = new List<role>();
            try
            {
                using(SqlCommand cmd=new SqlCommand("sp_role", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "getRoleById");
                    cmd.Parameters.AddWithValue("@id", id);
                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    using(SqlDataReader sdr= await cmd.ExecuteReaderAsync())
                    {
                        while(await sdr.ReadAsync())
                        {
                            list.Add(new role
                            {
                                id = Convert.ToInt32(sdr["id"]),
                                rolename = sdr["roleName"] != DBNull.Value ? sdr["roleName"].ToString():""
                            });
                        }
                    }
                }
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
            return list;
        }

        public async Task<bool> deleteRoleById(int id)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_role", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "deleteRole");
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

        #region department service


        public async Task<bool> insertDepartment(department data)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_department", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", data.id);
                    cmd.Parameters.AddWithValue("@departmentName", data.departmentName);
                    cmd.Parameters.AddWithValue("@action", data.id > 0 ? "updatedDepartment" : "insert");

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


        public async Task<(List<department> Data, int TotalRecords)> getAllDepartment(string? searchTerm,int? pageNumber,int? pageSize)
        {
            List<department> list = new();
            int totalRecords = 0;

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_department", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "getAllDepartment");

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
                            list.Add(new department
                            {
                                id = Convert.ToInt32(sdr["id"]),
                                departmentName = sdr["departmentName"]?.ToString()
                            });
                        }

                       
                        if (await sdr.NextResultAsync() && await sdr.ReadAsync())
                        {
                            totalRecords = Convert.ToInt32(sdr["TotalRecords"]);
                        }
                    }
                }
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }

            return (list, totalRecords);
        }

        public async Task<List<department>> getAllDepartmentById(int id)
        {
            List<department> list = new List<department>();
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_department", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "getDepartmentById");
                    cmd.Parameters.AddWithValue("@id", id);
                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await sdr.ReadAsync())
                        {
                            list.Add(new department
                            {
                                id = Convert.ToInt32(sdr["id"]),
                                departmentName = sdr["departmentName"] != DBNull.Value ? sdr["departmentName"].ToString() : ""
                            });
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
                    await _conn.CloseAsync();
            }
            return list;
        }


        public async Task<bool> deleteDepartmentById(int id)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_department", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "deleteDepartmentById");
                    cmd.Parameters.AddWithValue("@id", id);

                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Error deleting department", ex);
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    _conn.CloseAsync();
            }
        }
        #endregion

        #region designation
        public async Task<bool> InsertDesignation(designation data)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_designation", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@id", data.id);
                    cmd.Parameters.AddWithValue("@designationName", data.designationName);
                    cmd.Parameters.AddWithValue("@action", data.id > 0 ? "updateDesignation" : "insert");

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


        public async Task<(List<designation> Data, int TotalRecords)> getAllDesignation(string? searchTerm,int? pageNumber,int? pageSize)
        {
            List<designation> list = new();
            int totalRecords = 0;

            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_designation", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "getAllDesignation");

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
                            list.Add(new designation
                            {
                                id = Convert.ToInt32(sdr["id"]),
                                designationName = sdr["designationName"]?.ToString()
                            });
                        }

                    
                        if (await sdr.NextResultAsync() && await sdr.ReadAsync())
                        {
                            totalRecords = Convert.ToInt32(sdr["TotalRecords"]);
                        }
                    }
                }
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    await _conn.CloseAsync();
            }

            return (list, totalRecords);
        }


        public async Task<List<designation>> getAllDesignationById(int id)
        {
            List<designation> list = new List<designation>();
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_designation", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "getDesignationById");
                    cmd.Parameters.AddWithValue("@id", id);
                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    using (SqlDataReader sdr = await cmd.ExecuteReaderAsync())
                    {
                        while (await sdr.ReadAsync())
                        {
                            list.Add(new designation
                            {
                                id = Convert.ToInt32(sdr["id"]),
                                designationName = sdr["designationName"] != DBNull.Value ? sdr["designationName"].ToString() : ""
                            });
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
                    await _conn.CloseAsync();
            }
            return list;
        }

        public async Task<bool> deleteDesignationById(int id)
        {
            try
            {
                using (SqlCommand cmd = new SqlCommand("sp_designation", _conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@action", "deleteById");
                    cmd.Parameters.AddWithValue("@id", id);

                    if (_conn.State != ConnectionState.Open)
                        await _conn.OpenAsync();

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {

                throw new Exception("Error deleting designation", ex);
            }
            finally
            {
                if (_conn.State == ConnectionState.Open)
                    _conn.CloseAsync();
            }
        }
        #endregion
    }
}
