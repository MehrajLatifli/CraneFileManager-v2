using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore;
using System;
using Microsoft.Extensions.Configuration;
using System.IO;
using CraneFileManager.Domain.Entities.Configurations;

namespace CraneFileManager.Persistence.Contexts.CraneFileManagerDbContext
{
    public class CraneFileManagerContextFactory : IDesignTimeDbContextFactory<CraneFileManagerContext>
    {
        public CraneFileManagerContext CreateDbContext(string[] args)
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            // Create a new ConfigurationManager
            var configurationManager = new ConfigurationManager();

            // Load configuration based on environment
            if (env != null)
            {
                configurationManager.SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{env}.json", optional: true) // Load environment specific settings
                    .AddEnvironmentVariables(); // Add environment variables to configuration
            }
            else
            {
                configurationManager.AddJsonFile("appsettings.json", optional: true); // Default to appsettings.json if environment is null
            }

            // Get the connection string from the configuration
            var connectionString = configurationManager.GetConnectionString("CustomDbConnection");

            // Create a new AppSettings object and bind the configuration to it
            var appSettings = new AppSettings();
            configurationManager.GetSection("AppSettings").Bind(appSettings);

            // Create DbContext options using the connection string from the configuration
            var optionsBuilder = new DbContextOptionsBuilder<CraneFileManagerContext>();
            optionsBuilder.UseSqlServer(connectionString); // Use the connection string from the configuration

            // Return the DbContext instance, passing the options and appSettings
            return new CraneFileManagerContext(optionsBuilder.Options, appSettings);
        }
    }
}
