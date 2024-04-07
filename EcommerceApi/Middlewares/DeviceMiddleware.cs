using System.Net;
using Wangkanai.Detection.Models;
using Wangkanai.Detection.Services;

namespace EcommerceApi.Middlewares
{
    public class DeviceMiddleware
    {
        private readonly RequestDelegate _next;

        public DeviceMiddleware(RequestDelegate next)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
        }

        public async Task InvokeAsync(HttpContext context, IDetectionService detection)
        {
            if (detection.Device.Type == Device.Mobile)
            {
                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.NotImplemented;
                await context.Response.WriteAsJsonAsync(new
                {
                    message = "Don't support for mobile",
                    statusCode = 501,
                });
            }
            await _next(context);
        }

    }
}
