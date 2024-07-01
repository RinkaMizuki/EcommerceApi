using EcommerceApi.ExtensionExceptions;
using Microsoft.AspNetCore.Authentication;

namespace EcommerceApi.Middlewares
{
    public class AuthMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var defaultAuthResult = await context.AuthenticateAsync("SsoDefaultSchema");
            //var fbAuthResult = await context.AuthenticateAsync("SsoFacebookSchema");
            //var ggAuthResult = await context.AuthenticateAsync("SsoGoogleSchema");

            if(defaultAuthResult.Failure is HttpStatusException)
            {
                var RevokeTokenException = (HttpStatusException)defaultAuthResult.Failure;
                if((int)RevokeTokenException.Status! == 403)
                {
                    context.Response.Clear();
                    context.Response.ContentType = "text/plain";
                    context.Response.StatusCode = (int)RevokeTokenException.Status!;
                    await context.Response.WriteAsJsonAsync(new
                    {
                        message = RevokeTokenException.Message,
                        statusCode = RevokeTokenException.Status
                    });
                    return;
                }
            }
            // && !fbAuthResult.Succeeded && !ggAuthResult.Succeeded  || !fbAuthResult.None || !ggAuthResult.None
            if ((!defaultAuthResult.Succeeded) && (!defaultAuthResult.None))
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
