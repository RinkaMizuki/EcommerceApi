using Asp.Versioning;
using EcommerceApi.Attributes;
using EcommerceApi.Authentication;
using EcommerceApi.Config;
using EcommerceApi.Extensions;
using EcommerceApi.FilterBuilder;
using EcommerceApi.Hubs;
using EcommerceApi.Lib;
using EcommerceApi.Middleware;
using EcommerceApi.Middlewares;
using EcommerceApi.Models;
using EcommerceApi.Models.IdentityData;
using EcommerceApi.Services;
using EcommerceApi.Services.AddressService;
using EcommerceApi.Services.BackgroundTaskService;
using EcommerceApi.Services.CacheService;
using EcommerceApi.Services.CategoryService;
using EcommerceApi.Services.ChatService;
using EcommerceApi.Services.ConfirmService;
using EcommerceApi.Services.ContactService;
using EcommerceApi.Services.CouponService;
using EcommerceApi.Services.DestinateService;
using EcommerceApi.Services.EmailService;
using EcommerceApi.Services.FeedbackRateService;
using EcommerceApi.Services.FeedbackService;
using EcommerceApi.Services.InvoiceService;
using EcommerceApi.Services.MailService;
using EcommerceApi.Services.MerchantService;
using EcommerceApi.Services.OpenaiService;
using EcommerceApi.Services.OrderService;
using EcommerceApi.Services.PaymentService;
using EcommerceApi.Services.ProductService;
using EcommerceApi.Services.RedisService;
using EcommerceApi.Services.SegmentService;
using EcommerceApi.Services.SliderService;
using EcommerceApi.Services.SsoService;
using EcommerceApi.Shared;
using EcommerceApi.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
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
builder.Services.AddSignalR();
builder.Services.AddSwaggerGen(options => {
    options.OperationFilter<SwaggerDefaultValues>();
});
builder.Services.AddSingleton<PayOsLibrary>();
builder.Services.AddSingleton<IConnectionMultiplexer>(option =>
{
    var connectionStrings = configuration.GetSection("Redis")["ConnectionStrings"]!;
    var redisConfig = ConfigurationOptions.Parse(connectionStrings, true);
    redisConfig.AbortOnConnectFail = false;
    return ConnectionMultiplexer.Connect(connectionStrings);
});

builder.Services.AddSingleton<AdminConnection>();
builder.Services.AddSingleton<IRedisService, RedisService>();
builder.Services.AddSingleton<IUserIdProvider, CustomUserIdProvider>();
builder.Services.AddScoped<JwtMiddleware>();
builder.Services.AddScoped<AuthMiddleware>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICloudflareClientService, CloudflareClientService>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.Decorate<IProductService, CacheProductService>(); // ensure receives the expected ProductService
builder.Services.AddScoped<IRateService, RateService>();
builder.Services.AddScoped<IFeedbackService, FeedbackService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<ICouponService, CouponService>();
builder.Services.AddScoped<ISegmentService, SegmentService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ISliderService, SliderService>();
builder.Services.AddScoped<IAddressService, AddressService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IMerchantService, MerchantService>();
builder.Services.AddScoped<IDestinateService, DestinateService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
builder.Services.AddScoped<IConversationService, ConversationService>();
builder.Services.AddScoped<IMessageService, MessageService>();
builder.Services.AddScoped<RateFilterBuilder>();
builder.Services.AddScoped<ProductFilterBuilder>();
builder.Services.AddScoped<OrderFilterBuilder>();
builder.Services.AddScoped<UserFilterBuilder>();
builder.Services.AddScoped<InvoiceFilterBuilder>();
builder.Services.AddScoped<IConfirmService, ConfirmService>();
builder.Services.AddTransient<IConfigureOptions<SwaggerGenOptions>, ConfigureSwaggerOptions>();
builder.Services.AddScoped<IMailService, MailService>();
builder.Services.AddScoped<IOpenaiService, OpenaiService>();
builder.Services.AddScoped<ISsoService, SsoService>();
builder.Services.AddScoped<IAuthorizationHandler, AuthorizationPermissionHandler>();
builder.Services.AddHostedService<QueuedHostedService>();
builder.Services.AddSingleton<IProductTaskQueueSerivce, ProductTaskQueueService>();
//builder.Services.AddHostedService<ExpiredUserCleanupService>();

//binding config appsettings.json
builder.Services.Configure<CloudflareR2Config>(configuration.GetSection("CloudflareR2Config"));
builder.Services.Configure<EmailConfig>(configuration.GetSection("EmailConfiguration"));
builder.Services.Configure<VnPayConfig>(configuration.GetSection("VnPayConfiguration"));
builder.Services.Configure<PayOsConfig>(configuration.GetSection("PayOsConfiguration"));
builder.Services.Configure<HostOptions>(option =>
{
    option.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
});

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

builder.Services.AddAuthentication().AddScheme<TokenAuthSchemeOptions, AuthenticationDefaultHandler>(
    "SsoDefaultSchema",
    opts => { }
).AddScheme<TokenAuthSchemeOptions, AuthenticationFacebookHandler>(
    "SsoFacebookSchema",
    opts => { }
).AddScheme<TokenAuthSchemeOptions, AuthenticationGoogleHandler>(
    "SsoGoogleSchema",
    opts => { }
);
builder.Services.AddAuthorization(options =>
{
    options.DefaultPolicy = new AuthorizationPolicyBuilder() //default [Authorize]
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes("SsoDefaultSchema", "SsoFacebookSchema", "SsoGoogleSchema") //custom Facebook, Google Schema 
            .Build();
    options.AddPolicy("SsoAdmin", new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddAuthenticationSchemes("SsoDefaultSchema")
            .AddRequirements(new AdminAccessApiRequirement(configuration))
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

//app.UseMiddleware<DeviceMiddleware>();
app.UseMiddleware<JwtMiddleware>();

app.UseAuthentication();
app.UseMiddleware<AuthMiddleware>();
app.UseAuthorization();

app.MapHub<OrderHub>("/api/v1/admin/orderhub").RequireAuthorization("SsoAdmin");
app.MapHub<ChatHub>("/api/v1/admin/chathub");

app.MapControllers();

app.Run();


//options =>
//{
//    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//}
//.AddJwtBearer("Default", options =>
//{
//    options.IncludeErrorDetails = true;
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidAudience = configuration.GetSection("JwtConfiguration:ValidAudience").Value,
//        ValidIssuer = configuration.GetSection("JwtConfiguration:ValidIssuer").Value,
//        ClockSkew = TimeSpan.Zero,
//        IssuerSigningKey =
//            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configure.GetSection("JwtConfiguration:Secret").Value ?? ""))
//    };
//})
//.AddJwtBearer("Google", options =>
//{
//    var certificates = Helpers.Certificates.Value;
//    options.IncludeErrorDetails = true;
//    options.TokenValidationParameters = new TokenValidationParameters()
//    {
//        ValidateIssuerSigningKey = true,
//        RequireSignedTokens = true,
//        ValidateIssuer = true,
//        ValidateActor = false,
//        ValidateAudience = true,
//        ValidAudience = configuration.GetSection("GoogleConfiguration:ClientId").Value,
//        ValidIssuer = configuration.GetSection("GoogleConfiguration:GoogleIssuer").Value,
//        IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
//        {
//            return certificates
//            .Where(x => x.Key?.ToUpper() == kid?.ToUpper())
//            .Select(x => new X509SecurityKey(x.Value));
//        },
//        IssuerSigningKeys = certificates.Values.Select(x => new X509SecurityKey(x)),
//        ValidateLifetime = true,
//        RequireExpirationTime = true,
//    };
//}).AddJwtBearer("Facebook", options =>
//{
//    options.IncludeErrorDetails = true;
//    options.TokenValidationParameters = new TokenValidationParameters()
//    {
//        ValidateIssuer = true,
//        ValidateAudience = true,
//        ValidAudience = configuration.GetSection("FacebookConfiguration:AppId").Value,
//        ValidIssuer = configuration.GetSection("FacebookConfiguration:FacebookIssuer").Value,
//        ClockSkew = TimeSpan.Zero, //apply when validate time of token
//        IssuerSigningKey =
//            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configure.GetSection("FacebookConfiguration:AppSecret").Value ?? ""))
//    };
//})
//options.AddPolicy(IdentityData.AdminPolicyName, new AuthorizationPolicyBuilder()
//        .RequireAuthenticatedUser()
//        .AddAuthenticationSchemes("Default")
//        .RequireClaim("Role", IdentityData.AdminPolicyRole)
//        .Build()
//);