using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace FluencyHub.ContentManagement.Infrastructure.Persistence;

public class ContentDbContextFactory : IDesignTimeDbContextFactory<ContentDbContext>
{
    public ContentDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ContentDbContext>();
        optionsBuilder.UseSqlite("Data Source=../../../../API/Data/fluencyhub_content.db");

        return new ContentDbContext(optionsBuilder.Options);
    }
} 