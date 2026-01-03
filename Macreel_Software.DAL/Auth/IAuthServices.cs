using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Macreel_Software.Models;
using Microsoft.AspNetCore.Http;

namespace Macreel_Software.DAL.Auth
{
    public interface IAuthServices
    {

        public Task<UserData?> ValidateUserAsync(string userName, string password);
        public Task<bool> SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiry);
        Task<string> UploadFileAsync(IFormFile file, string folderPath, string[] allowedExtensions = null, long maxFileSize = 10485760);
    }
    
}
