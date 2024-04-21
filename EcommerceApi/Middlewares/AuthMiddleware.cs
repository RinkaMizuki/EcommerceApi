using Microsoft.AspNetCore.Authentication;

namespace EcommerceApi.Middlewares
{
    public class AuthMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var defaultAuthResult = await context.AuthenticateAsync("SsoDefaultSchema");
            var fbAuthResult = await context.AuthenticateAsync("SsoFacebookSchema");
            var ggAuthResult = await context.AuthenticateAsync("SsoGoogleSchema");

            if ((!defaultAuthResult.Succeeded && !fbAuthResult.Succeeded && !ggAuthResult.Succeeded) && (!defaultAuthResult.None || !fbAuthResult.None || !ggAuthResult.None))
            {
                context.Response.Clear();
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = 401;
                await context.Response.WriteAsJsonAsync(new { 
                    message = "Unauthorized",
                    statusCode = 401
                });
                return;
            }
            await next(context);

        }
    }
}
