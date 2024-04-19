using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentApproval_DTOs
{
    public class GenericResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public T Data { get; set; }

        public GenericResponse(T data, string message = null!, bool success = true)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static GenericResponse<T> SuccessResponse(T data, string message = null!)
        {
            return new GenericResponse<T>(data, message, true);
        }

        public static GenericResponse<T> ErrorResponse(string message, bool success = false)
        {
            return new GenericResponse<T>(default(T)!, message, success);
        }
    }
}
