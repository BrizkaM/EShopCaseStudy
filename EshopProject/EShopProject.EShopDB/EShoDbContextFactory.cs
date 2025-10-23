//------------------------------------------------------------------------------------------
// File: EshopDbContextFactory.cs
//------------------------------------------------------------------------------------------
using EShopProject.EShopDB.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

/// <summary>
/// Factory class for creating EShopDbContext instances at design time.
/// Used by Entity Framework Core tools for migrations and database updates.
/// </summary>
public class EshopDbContextFactory : IDesignTimeDbContextFactory<EShopDbContext>
{
    /// <summary>
    /// Creates a new instance of EShopDbContext.
    /// Reads connection string from appsettings.json in the WebApi project.
    /// </summary>
    /// <param name="args">Command line arguments (not used)</param>
    /// <returns>A new instance of EShopDbContext configured with the application's connection string</returns>
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