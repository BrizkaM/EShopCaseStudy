using EShopProject.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

public class EshopDbContextFactory : IDesignTimeDbContextFactory<EShopDbContext>
{
    public EShopDbContext CreateDbContext(string[] args)
    {
        var apiProjectPath = Path.Combine(
            Directory.GetCurrentDirectory(),
            "../EShopProject.WebApi"
        );

        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(apiProjectPath)
            .AddJsonFile("appsettings.json", optional: false)
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        var builder = new DbContextOptionsBuilder<EShopDbContext>();
        builder.UseSqlServer(connectionString);

        return new EShopDbContext(builder.Options);
    }
}