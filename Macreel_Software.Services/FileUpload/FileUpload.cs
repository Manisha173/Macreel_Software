
using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Macreel_Software.Services.FileUpload.Services
{
    

    public class FileUploadService 
    {
        public async Task<string> UploadFileAsync(IFormFile file, string folderPath, string[] allowedExtensions = null, long maxFileSize = 10485760)
        {
            if (file == null || file.Length == 0)
                throw new Exception("File is empty.");

       
            if (file.Length > maxFileSize)
                throw new Exception($"File size exceeds the allowed limit of {maxFileSize / (1024 * 1024)} MB.");

          
            var extension = Path.GetExtension(file.FileName);
            if (allowedExtensions != null && allowedExtensions.Length > 0)
            {
                bool isValidExt = false;
                foreach (var ext in allowedExtensions)
                {
                    if (string.Equals(ext, extension, StringComparison.OrdinalIgnoreCase))
                    {
                        isValidExt = true;
                        break;
                    }
                }
                if (!isValidExt)
                    throw new Exception($"File type {extension} is not allowed.");
            }
            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folderPath, uniqueFileName);

         
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return uniqueFileName; 
        }
    }
}
