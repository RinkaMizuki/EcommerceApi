using System.Text;
using Asp.Versioning;
using EcommerceApi;
using EcommerceApi.Config;
using EcommerceApi.FilterBuilder;
using EcommerceApi.Middleware;
using EcommerceApi.Middlewares;
using EcommerceApi.Models;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services;
using EcommerceApi.Services.AddressService;
using EcommerceApi.Services.CacheService;
using EcommerceApi.Services.CategoryService;
using EcommerceApi.Services.ConfirmService;
using EcommerceApi.Services.ContactService;
using EcommerceApi.Services.CouponService;
using EcommerceApi.Services.DestinateService;
using EcommerceApi.Services.EmailService;
using EcommerceApi.Services.FeedbackRateService;
using EcommerceApi.Services.FeedbackService;
using EcommerceApi.Services.MailService;
using EcommerceApi.Services.MerchantService;
using EcommerceApi.Services.OpenaiService;
using EcommerceApi.Services.OrderService;
using EcommerceApi.Services.PaymentService;
using EcommerceApi.Services.ProductService;
using EcommerceApi.Services.SegmentService;
using EcommerceApi.Services.SliderService;
using EcommerceApi.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Org.BouncyCastle.Tls;
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

//config response xml
//builder.Services.AddControllers().AddXmlDataContractSerializerFormatters();

var configuration = builder.Configuration;

// Add services to the container.

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
builder.Services.AddDetection();

builder.Services.AddSwaggerGen(options => {
    options.OperationFilter<SwaggerDefaultValues>();
});
builder.Services.AddTransient<JwtMiddleware>();
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
builder.Services.AddTransient<IOrderService, OrderService>();
builder.Services.AddTransient<ISliderService, SliderService>();
builder.Services.AddTransient<IAddressService, AddressService>();
builder.Services.AddTransient<IPaymentService, PaymentService>();
builder.Services.AddTransient<IMerchantService, MerchantService>();
builder.Services.AddTransient<IDestinateService, DestinateService>();
builder.Services.AddScoped<RateFilterBuilder>();
builder.Services.AddTransient<IConfirmService, ConfirmService>();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddTransient<IMailService, MailService>();
builder.Services.AddTransient<IOpenaiService, OpenaiService>();

//binding config appsettings.json
builder.Services.Configure<CloudflareR2Config>(configuration.GetSection("CloudflareR2Config"));
builder.Services.Configure<EmailConfig>(configuration.GetSection("EmailConfiguration"));
builder.Services.Configure<VnPayConfig>(configuration.GetSection("VnPayConfiguration"));
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyAllowSpecificOrigins",
        policy =>
        {
            policy
                .SetIsOriginAllowed(_ => true)
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
}).AddJwtBearer("Default", options =>
{
    options.IncludeErrorDetails = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = configuration.GetSection("JwtConfiguration:ValidAudience").Value,
        ValidIssuer = configuration.GetSection("JwtConfiguration:ValidIssuer").Value,
        ClockSkew = TimeSpan.Zero,
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configure.GetSection("JwtConfiguration:Secret").Value ?? ""))
    };
}).AddJwtBearer("Google", options =>
{
    var certificates = Helpers.Certificates.Value;
    options.IncludeErrorDetails = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,
        RequireSignedTokens = true,
        ValidateIssuer = true,
        ValidateActor = false,
        ValidateAudience = true,
        ValidAudience = configuration.GetSection("GoogleConfiguration:ClientId").Value,
        ValidIssuer = configuration.GetSection("GoogleConfiguration:GoogleIssuer").Value,
        IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
        {
            return certificates
            .Where(x => x.Key?.ToUpper() == kid?.ToUpper())
            .Select(x => new X509SecurityKey(x.Value));
        },
        IssuerSigningKeys = certificates.Values.Select(x => new X509SecurityKey(x)),
        ValidateLifetime = true,
        RequireExpirationTime = true,
    };
}).AddJwtBearer("Facebook", options =>
{
    options.IncludeErrorDetails = true;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidAudience = configuration.GetSection("FacebookConfiguration:AppId").Value,
        ValidIssuer = configuration.GetSection("FacebookConfiguration:FacebookIssuer").Value,
        ClockSkew = TimeSpan.Zero, //apply when validate time of token
        IssuerSigningKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configure.GetSection("FacebookConfiguration:AppSecret").Value ?? ""))
    };
});

builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes("Default", "Facebook", "Google")
            .Build();

    options.AddPolicy(IdentityData.AdminPolicyName, new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes("Default")
            .RequireClaim("Role", IdentityData.AdminPolicyRole)
            .Build()
    );
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
app.UseDetection();
app.UseCors("MyAllowSpecificOrigins");

app.UseMiddleware<DeviceMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();