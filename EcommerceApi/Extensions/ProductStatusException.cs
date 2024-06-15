using System.Net;

namespace EcommerceApi.Extensions
{
    public class ProductStatusException : Exception
    {
        public object? Result { get; private set; }
        public HttpStatusCode Status { get; private set; }
        public ProductStatusException(HttpStatusCode httpStatus, string msg, object result) 
            : base(msg)
        {
            Status = httpStatus;
            Result = result;
        }
    }
}
