using Amazon.Runtime.Internal;
using EcommerceApi.ExtensionExceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace EcommerceApi.Controllers.V1
{
    [ApiController]
    [AllowAnonymous]
    [ApiExplorerSettings(IgnoreApi = true)] //is needed. Otherwise, it may break your Swashbuckle swagger
    public class ErrorController : ControllerBase
    {
        [Route("error")]
        public IActionResult Error()
        {
            var context = HttpContext
                                    .Features
                                    .Get<IExceptionHandlerFeature>();
            var exception = context?.Error;

            if(exception is HttpStatusException httpException)
            {
                Response.StatusCode = Convert.ToInt32(httpException?.Status);
                return new JsonResult(new
                {
                    message = httpException?.Message,
                    statusCode = Convert.ToInt32(httpException?.Status),
                });
            }
            //InnerException = {"Exception of type 'Amazon.Runtime.Internal.HttpErrorResponseException' was thrown."}
            else if (exception?.InnerException is HttpErrorResponseException)
            {
                var AwsException = (Amazon.S3.AmazonS3Exception)exception;
                Response.StatusCode = Convert.ToInt32(AwsException.StatusCode);
                return new JsonResult(new
                {
                    message = AwsException.Message,
                    statusCode = AwsException.StatusCode,
                });
            }
            else
            {
                return new JsonResult(new
                {
                    message = "Has been an error on the server",
                    statusCode = 500,
                });
            }
        }
    }
}
