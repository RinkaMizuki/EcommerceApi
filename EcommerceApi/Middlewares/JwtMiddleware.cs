namespace EcommerceApi.Middleware;

public class JwtMiddleware : IMiddleware
{
    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var accessToken = context.Request.Cookies["accessToken"];
        if (!string.IsNullOrEmpty(accessToken) && !context.Request.Headers.ContainsKey("Authorization"))
        {
            context.Request.Headers.Append("Authorization", $"Bearer {accessToken}");
        }
        return next(context);
    }
}