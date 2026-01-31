using Macreel_Software.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Macreel_Software.DAL.Auth.main;

namespace Macreel_Software.DAL.Auth
{
    public interface IAuthServices
    {
        Task<UserData?> ValidateUserAsync(string userName, string password);
        Task<bool> SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiry);
        Task<RefreshTokenData?> GetRefreshTokenAsync(string refreshToken);
        Task<UserData?> GetUserByIdAsync(int userId);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
        Task<int?> CheckUserExistOrNot(string email);
        Task<int?> GetUserIdByEmailId(string email);
        Task<bool> UpdatePassword(string encryptedPassword, int? userId);
    }
    
}
