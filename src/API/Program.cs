using FluencyHub.API;
using FluencyHub.API.Middleware;
using FluencyHub.StudentManagement.Application;
using FluencyHub.ContentManagement.Application;
using FluencyHub.PaymentProcessing.Application;
using FluencyHub.StudentManagement.Infrastructure;
using FluencyHub.ContentManagement.Infrastructure;
using FluencyHub.PaymentProcessing.Infrastructure;
using FluencyHub.SharedKernel;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços do SharedKernel
builder.Services.AddSharedKernelServices();

// Configurar MediatR com todos os assemblies que contêm handlers
var assemblies = new[]
{
    typeof(Program).Assembly, // API
    Assembly.GetAssembly(typeof(StudentManagementApplicationReference)) ?? throw new InvalidOperationException("StudentManagement.Application assembly not found"),
    Assembly.GetAssembly(typeof(ContentManagementApplicationReference)) ?? throw new InvalidOperationException("ContentManagement.Application assembly not found"),
    Assembly.GetAssembly(typeof(PaymentProcessingApplicationReference)) ?? throw new InvalidOperationException("PaymentProcessing.Application assembly not found"),
    Assembly.GetAssembly(typeof(SharedKernelReference)) ?? throw new InvalidOperationException("SharedKernel assembly not found")
};

builder.Services.AddMediatorServices(assemblies);

// Add services to the container
builder.Services.AddStudentManagementApplication();
builder.Services.AddContentManagementApplication();
builder.Services.AddPaymentProcessingApplication();

// Adicionar serviços de infraestrutura
builder.Services.AddStudentManagementInfrastructureServices(builder.Configuration);
builder.Services.AddContentManagementInfrastructureServices(builder.Configuration);
builder.Services.AddPaymentProcessingInfrastructureServices(builder.Configuration);

builder.Services.AddApiServices(builder.Configuration);

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerConfiguration();

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment() || app.Environment.EnvironmentName == "Testing")
{
    app.UseSwaggerConfiguration();
}

try
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();

    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    
    // Migrar os bancos de dados de cada contexto
    var contentDbContext = services.GetRequiredService<FluencyHub.ContentManagement.Infrastructure.Persistence.ContentDbContext>();
    var studentDbContext = services.GetRequiredService<FluencyHub.StudentManagement.Infrastructure.Persistence.StudentDbContext>();
    var paymentDbContext = services.GetRequiredService<FluencyHub.PaymentProcessing.Infrastructure.Persistence.PaymentDbContext>();
    
    logger.LogInformation("Aplicando migrações do banco de dados");
    
    await contentDbContext.Database.MigrateAsync();
    await studentDbContext.Database.MigrateAsync();
    await paymentDbContext.Database.MigrateAsync();
    
    logger.LogInformation("Migrações aplicadas com sucesso");
}
catch (Exception ex)
{
    var logger = app.Services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "Ocorreu um erro ao migrar ou inicializar o banco de dados");
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("/index.html");

app.Run();

// Making the Program class public and partial for testing
public partial class Program { }