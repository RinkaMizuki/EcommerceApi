using System.Net;

namespace EcommerceApi.ExtensionExceptions
{
    public class HttpStatusException : Exception
    {
        public HttpStatusCode? Status { get; private set; }
        public HttpStatusException(HttpStatusCode? httpStatus, string msg) 
            : base(msg) {
            Status = httpStatus;
        }
    }
}
