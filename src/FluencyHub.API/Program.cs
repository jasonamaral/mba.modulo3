using FluencyHub.API;
using FluencyHub.API.Middleware;
using FluencyHub.Application;
using FluencyHub.Infrastructure;
using FluencyHub.Infrastructure.Identity;
using FluencyHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddApiServices(builder.Configuration);
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddlewareConfiguration(app.Environment);
app.UseSwaggerConfiguration();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
    
    // Migrar o banco de dados automaticamente
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<FluencyHubDbContext>();
        dbContext.Database.Migrate();
    }
}

app.MapControllers();

// Seed database
try
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogInformation("Iniciando seed do banco de dados...");
    await DatabaseSeeder.SeedData(app.Services);
    logger.LogInformation("Seed do banco de dados concluído com sucesso!");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Erro ao executar seed do banco de dados");
    throw;
}

app.Run();

// Make Program class public and partial for testing
public partial class Program { }

// Schema filter to remove read-only properties
public class RemoveReadOnlyPropertiesSchemaFilter : Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiSchema schema, Swashbuckle.AspNetCore.SwaggerGen.SchemaFilterContext context)
    {
        if (schema?.Properties == null || context.Type == null)
            return;

        var readOnlyProperties = context.Type.GetProperties()
            .Where(p => p.GetMethod?.IsPublic == true && p.SetMethod?.IsPublic == false);

        foreach (var property in readOnlyProperties)
        {
            var propertyName = char.ToLowerInvariant(property.Name[0]) + property.Name.Substring(1);
            if (schema.Properties.ContainsKey(propertyName))
            {
                schema.Properties[propertyName].ReadOnly = true;
            }
        }
    }
}

// Schema filter to handle enums correctly
public class EnumSchemaFilter : Swashbuckle.AspNetCore.SwaggerGen.ISchemaFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiSchema schema, Swashbuckle.AspNetCore.SwaggerGen.SchemaFilterContext context)
    {
        if (context.Type.IsEnum)
        {
            schema.Enum.Clear();
            Enum.GetNames(context.Type)
                .ToList()
                .ForEach(name => schema.Enum.Add(new Microsoft.OpenApi.Any.OpenApiString(name)));
        }
    }
}

// Document filter to remove unused components
public class RemoveUnusedComponentsDocumentFilter : Swashbuckle.AspNetCore.SwaggerGen.IDocumentFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiDocument swaggerDoc, Swashbuckle.AspNetCore.SwaggerGen.DocumentFilterContext context)
    {
        // Get all referenced schemas
        var referencedSchemas = new HashSet<string>();

        // Check all operations and their responses/parameters
        foreach (var path in swaggerDoc.Paths.Values)
        {
            foreach (var operation in path.Operations.Values)
            {
                // Check parameters
                foreach (var parameter in operation.Parameters)
                {
                    if (parameter.Schema?.Reference != null)
                    {
                        referencedSchemas.Add(parameter.Schema.Reference.Id);
                    }
                }

                // Check request bodies
                if (operation.RequestBody?.Content != null)
                {
                    foreach (var content in operation.RequestBody.Content.Values)
                    {
                        if (content.Schema?.Reference != null)
                        {
                            referencedSchemas.Add(content.Schema.Reference.Id);
                        }
                    }
                }

                // Check responses
                foreach (var response in operation.Responses.Values)
                {
                    if (response.Content != null)
                    {
                        foreach (var content in response.Content.Values)
                        {
                            if (content.Schema?.Reference != null)
                            {
                                referencedSchemas.Add(content.Schema.Reference.Id);
                            }
                        }
                    }
                }
            }
        }

        // Remove unreferenced schemas
        var schemasToRemove = swaggerDoc.Components.Schemas
            .Where(x => !referencedSchemas.Contains(x.Key))
            .ToList();

        foreach (var schema in schemasToRemove)
        {
            swaggerDoc.Components.Schemas.Remove(schema.Key);
        }
    }
}

// Document filter to exclude domain types
public class ExcludeDomainTypesDocumentFilter : Swashbuckle.AspNetCore.SwaggerGen.IDocumentFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiDocument swaggerDoc, Swashbuckle.AspNetCore.SwaggerGen.DocumentFilterContext context)
    {
        var domainNamespaces = new[]
        {
            "FluencyHub.Domain"
        };

        var schemasToRemove = swaggerDoc.Components.Schemas
            .Where(x => domainNamespaces.Any(ns => x.Key.StartsWith(ns)))
            .ToList();

        foreach (var schema in schemasToRemove)
        {
            swaggerDoc.Components.Schemas.Remove(schema.Key);
        }
    }
}

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}