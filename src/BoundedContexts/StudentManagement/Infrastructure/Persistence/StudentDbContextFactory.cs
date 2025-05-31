using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace FluencyHub.StudentManagement.Infrastructure.Persistence;

public class StudentDbContextFactory : IDesignTimeDbContextFactory<StudentDbContext>
{
    public StudentDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../../../API"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<StudentDbContext>();
        var connectionString = configuration.GetConnectionString("StudentManagementConnection")
            ?? "Data Source=../../../API/studentmanagement.db";

        optionsBuilder.UseSqlite(connectionString);

        return new StudentDbContext(optionsBuilder.Options);
    }
} 