using System.Text;
using FluencyHub.Application;
using FluencyHub.Application.Common.Interfaces;
using FluencyHub.Infrastructure;
using FluencyHub.Infrastructure.Identity;
using FluencyHub.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using FluencyHub.API.Middleware;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using System.Reflection;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers()
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
        options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        options.SerializerSettings.Converters.Add(new StringEnumConverter());
    });

// Add application layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);

// Configure Swagger for API documentation
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "FluencyHub API", 
        Version = "v1",
        Description = "API para o Sistema de Aprendizagem de Idiomas FluencyHub"
    });
    
    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme."
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
    
    // Include XML comments if they exist
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
    
    // Customize schema generation
    c.CustomSchemaIds(type => type.FullName);
    c.UseAllOfForInheritance();
    c.UseOneOfForPolymorphism();
    c.SchemaFilter<RemoveReadOnlyPropertiesSchemaFilter>();
    c.SchemaFilter<EnumSchemaFilter>();
    c.DocumentFilter<RemoveUnusedComponentsDocumentFilter>();
    
    // Comment out this line as it may be filtering out controller methods
    // c.DocumentFilter<ExcludeDomainTypesDocumentFilter>();
});

// Custom schema filters for Swagger
builder.Services.AddTransient<RemoveReadOnlyPropertiesSchemaFilter>();
builder.Services.AddTransient<EnumSchemaFilter>();
builder.Services.AddTransient<RemoveUnusedComponentsDocumentFilter>();
builder.Services.AddTransient<ExcludeDomainTypesDocumentFilter>();

// CORS configuration
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

// Ensure databases are created (without running migrations during development)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<FluencyHubDbContext>();
        if (context.Database.EnsureCreated())
        {
            logger.LogInformation("Domain database created successfully");
        }

        var identityContext = services.GetRequiredService<ApplicationDbContext>();
        if (identityContext.Database.EnsureCreated())
        {
            logger.LogInformation("Identity database created successfully");
        }

        // Seed roles
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var roles = new[] { "Administrator", "Student" };
        
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                logger.LogInformation("Creating role: {Role}", role);
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Seed default student
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var studentRepository = services.GetRequiredService<FluencyHub.Application.Common.Interfaces.IStudentRepository>();
        
        // Create admin user if not exists
        var adminEmail = "admin@fluencyhub.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                FirstName = "Admin",
                LastName = "User",
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(adminUser, "Pass@word1");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Administrator");
                logger.LogInformation("Admin user created successfully");
            }
        }
        
        // Create student user if not exists
        var studentEmail = "student@example.com";
        var studentUser = await userManager.FindByEmailAsync(studentEmail);
        if (studentUser == null)
        {
            studentUser = new ApplicationUser
            {
                UserName = studentEmail,
                Email = studentEmail,
                FirstName = "Test",
                LastName = "Student",
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(studentUser, "Student123!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(studentUser, "Student");
                logger.LogInformation("Student user created successfully");
                
                // Create student entity in domain database
                var student = new FluencyHub.Domain.StudentManagement.Student(
                    studentUser.FirstName,
                    studentUser.LastName,
                    studentUser.Email,
                    DateTime.Now.AddYears(-20),
                    "+1234567890"
                );
                
                await studentRepository.AddAsync(student);
                await studentRepository.SaveChangesAsync();
                logger.LogInformation("Student entity created successfully with ID: {StudentId}", student.Id);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while creating or initializing the database");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options => 
    {
        options.SerializeAsV2 = false;
    });
    
    app.UseSwaggerUI(c => 
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "FluencyHub API v1");
        c.RoutePrefix = string.Empty; // Serve Swagger UI na raiz
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List); // Show operations expanded
        c.DefaultModelsExpandDepth(0); // Hide schemas section by default
        c.EnableDeepLinking();
        c.DisplayRequestDuration();
    });
    
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionHandlingMiddleware>();

app.MapControllers();

app.Run();

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

// Make Program class public and partial for testing
public partial class Program { }

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
