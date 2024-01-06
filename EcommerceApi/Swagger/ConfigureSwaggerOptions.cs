using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace EcommerceApi.Swagger;

public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
{
    private readonly IApiVersionDescriptionProvider _apiVersionDescriptionProvider;

    public ConfigureSwaggerOptions(IApiVersionDescriptionProvider apiVersionDescriptionProvider) => _apiVersionDescriptionProvider = apiVersionDescriptionProvider;
    private static OpenApiInfo CreateOpenApiInfo(ApiVersionDescription description)
    {
        var info = new OpenApiInfo()
        {
            Title = "LSE.Stocks.API",
            Version = description.ApiVersion.ToString()
        };

        if (description.IsDeprecated)
        {
            info.Description += " (deprecated)";
        }

        return info;
    }
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in _apiVersionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, CreateOpenApiInfo(description));
        }
        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
        {
            In = ParameterLocation.Header,
            Description = "Please provide valid token",
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            BearerFormat = "JWT",
            Scheme = "Bearer",
        });
        options.AddSecurityRequirement(new OpenApiSecurityRequirement()
        {
            {
                new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference()
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer",
                    },
                },
                Array.Empty<string>()
            }
        });
    }
}