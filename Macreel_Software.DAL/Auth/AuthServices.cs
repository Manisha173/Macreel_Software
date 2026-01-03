using System.Data;
using Macreel_Software.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Macreel_Software.DAL.Auth
{
    public class AuthServices : IAuthServices
    {
        private readonly IConfiguration _config;
         
        public AuthServices(IConfiguration config)
        {
            _config = config;
        }

        public async Task<UserData?> ValidateUserAsync(string userName, string password)
        {
           string message = string.Empty;
            UserData? user = null;

            try
            {
                using SqlConnection con = new SqlConnection(
                    _config.GetConnectionString("DefaultConnection"));

                using SqlCommand cmd = new SqlCommand("sp_Login", con);
                cmd.CommandType = CommandType.StoredProcedure;

                cmd.Parameters.AddWithValue("@UserName", userName);
                cmd.Parameters.AddWithValue("@Password", password);
                cmd.Parameters.AddWithValue("@Action", "LOGIN");

                await con.OpenAsync();

                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

                if (await dr.ReadAsync())
                {
                    if (dr["Password"].ToString() == password)
                    {
                        user = new UserData
                        {
                            UserId = Convert.ToInt32(dr["UserId"]),
                            Username = dr["UserName"].ToString(),
                            Role = dr["Role"].ToString()
                        };
                    }
                    else
                    {
                        message = "Password does not match";
                    }
                }
                else
                {
                    message = "Invalid credentials";
                }
            }
        
            catch (Exception ex)
            {
                message = "Something went wrong";
             
            }

            return user;
        }


        public async Task<bool> SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiry)
        {
            try
            {
                using SqlConnection con = new SqlConnection(_config.GetConnectionString("DefaultConnection"));
                using SqlCommand cmd = new SqlCommand("sp_Login", con)
                {
                    CommandType = CommandType.StoredProcedure
                };

                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@RefreshToken", refreshToken);
                cmd.Parameters.AddWithValue("@ExpireDate", expiry);
                cmd.Parameters.AddWithValue("@Action", "UPDATE_REFRESH");

                await con.OpenAsync();
                int rowsAffected = await cmd.ExecuteNonQueryAsync();

                return rowsAffected > 0; 
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public Task<string> UploadFileAsync(IFormFile file, string folderPath, string[] allowedExtensions = null, long maxFileSize = 10485760)
        {
            throw new NotImplementedException();
        }
    }
}
