using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;

namespace FluencyHub.API;

public static class SwaggerConfiguration
{
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "FluencyHub API",
                Version = "v1",
                Description = "API para o Sistema de Aprendizado de Idiomas FluencyHub",
                Contact = new OpenApiContact
                {
                    Name = "Jason Santos do Amaral"
                }
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Cabeçalho de autorização JWT usando o esquema Bearer."
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            c.ExampleFilters();

            c.EnableAnnotations();
            c.CustomSchemaIds(type => type.FullName);
            c.UseAllOfForInheritance();
            c.UseOneOfForPolymorphism();
        });

        services.AddSwaggerExamplesFromAssemblyOf<Program>();

        return services;
    }

    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "swagger/{documentName}/swagger.json";
        });

        app.UseSwaggerUI(c =>
        {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "FluencyHub API v1");
            c.RoutePrefix = string.Empty;
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
            c.DefaultModelsExpandDepth(0);
            c.EnableDeepLinking();
            c.DisplayRequestDuration();
        });

        return app;
    }
} 