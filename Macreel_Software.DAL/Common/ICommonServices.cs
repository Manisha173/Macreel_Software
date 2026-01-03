

using Microsoft.AspNetCore.Http;

namespace Macreel_Software.DAL.Common
{
    public interface ICommonServices
    {
        string UploadImage(IFormFile file, string folderName);
    }
}
