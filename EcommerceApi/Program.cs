using System.Text;
using Asp.Versioning;
using EcommerceApi.Config;
using EcommerceApi.Extensions;
using EcommerceApi.Middleware;
using EcommerceApi.Models;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services;
using EcommerceApi.Services.CacheService;
using EcommerceApi.Services.CategoryService;
using EcommerceApi.Services.ContactService;
using EcommerceApi.Services.CouponService;
using EcommerceApi.Services.EmailService;
using EcommerceApi.Services.FeedbackRateService;
using EcommerceApi.Services.FeedbackService;
using EcommerceApi.Services.MailService;
using EcommerceApi.Services.ProductService;
using EcommerceApi.Services.SegmentService;
using EcommerceApi.Swagger;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Swashbuckle.AspNetCore.SwaggerGen;

var builder = WebApplication.CreateBuilder(args);

var configure = builder.Configuration;
//Connect to DB
builder.Services.AddDbContext<EcommerceDbContext>(options =>
{
    var connectionString = configure.GetConnectionString("DefaultConnection");
    if (!string.IsNullOrEmpty(connectionString))
    {
        options.UseSqlServer(connectionString);
    }
    else
    {
        throw new Exception("Can't not connect to DB");
    }
});

var configuration = builder.Configuration;

// Add services to the container.
builder.Services.AddTransient<JwtMiddleware>();
//Add NewtonsoftJson Options fix ReferenceLoopHandling is currently not supported in the System.Text.Json serializer.
 //builder.Services.AddControllers().AddNewtonsoftJson(options =>
 //    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHealthChecksUI().AddInMemoryStorage();
builder.Services.AddHealthChecks()
                .AddSqlServer(configuration.GetConnectionString("DefaultConnection")!)
                .AddCheck<MyHealthCheck>("MyHealthCheck");
builder.Services.AddSwaggerGen(options => { 
    options.OperationFilter<SwaggerDefaultValues>();
});

builder.Services.AddTransient<IUserService, UserService>();
builder.Services.AddTransient<ICategoryService, CategoryService>();
builder.Services.AddTransient<ICloudflareClientService, CloudflareClientService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.Decorate<IProductService, CacheProductService>(); // ensure receives the expected ProductService
builder.Services.AddTransient<IRateService, RateService>();
builder.Services.AddTransient<IFeedbackService, FeedbackService>();
builder.Services.AddTransient<IContactService, ContactService>();
builder.Services.AddTransient<ICouponService, CouponService>();
builder.Services.AddTransient<ISegmentService, SegmentService>();

builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddTransient<IMailService, MailService>();
builder.Services.Configure<CloudflareR2Config>(configuration.GetSection("CloudflareR2Config"));
builder.Services.Configure<EmailConfig>(configuration.GetSection("EmailConfiguration"));

builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAllowSpecificOrigins",
        policy =>
        {
            policy
                .SetIsOriginAllowed(origin => true)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.IncludeErrorDetails = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = "http://localhost:5083",
        ValidIssuer = "http://localhost:5083",
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configure.GetSection("SecretKeyToken").Value ?? ""))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(IdentityData.AdminPolicyName,
        policy => { policy.RequireClaim("Role", IdentityData.AdminPolicyRole); });
});

var app = builder.Build();

app.UseExceptionHandler("/error"); // add middleware into pipeline to handle global exceptions

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        var descriptions = app.DescribeApiVersions();
        foreach (var description in descriptions)
        {
            options.SwaggerEndpoint($"/swagger/{description.GroupName}/swagger.json",
                $"Ecommerce_{description.GroupName.ToUpper()}");
        }
    });
}

app.UseHttpsRedirection();
app.UseCors("MyAllowSpecificOrigins");
app.UseMiddleware<JwtMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions()
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});

app.MapHealthChecksUI(options => {
    options.AddCustomStylesheet("dotnet.css");
});
app.MapControllers();

app.Run();