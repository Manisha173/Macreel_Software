using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Macreel_Software.Models
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public int StatusCode { get; set; }
        public string Message { get; set; }

        public T Data { get; set; }

        // Pagination
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
        public int? TotalRecords { get; set; }
        public int? TotalPages { get; set; }

        // Error
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
        public Dictionary<string, string[]> ValidationErrors { get; set; }

        public static ApiResponse<T> SuccessResponse(
            T data,
            string message = "Success",
            int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public static ApiResponse<T> PagedResponse(
            T data,
            int pageNumber,
            int pageSize,
            int totalRecords,
            string message = "Success",
            int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                Success = true,
                StatusCode = statusCode,
                Message = message,
                Data = data,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalRecords,
                TotalPages = (int)Math.Ceiling((double)totalRecords / pageSize)
            };
        }

        public static ApiResponse<T> FailureResponse(
            string message,
            int statusCode,
            string errorCode = null,
            Dictionary<string, string[]> validationErrors = null)
        {
            return new ApiResponse<T>
            {
                Success = false,
                StatusCode = statusCode,
                Message = message,
                ErrorCode = errorCode,
                ErrorMessage = message,
                ValidationErrors = validationErrors
            };
        }
    }
}
