using InkVault.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Npgsql;

namespace InkVault
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            
            // Get environment
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            
            // Try to get connection string from environment variable (for Render in production)
            var connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
            
            // If not in environment, use appsettings
            if (string.IsNullOrEmpty(connectionString))
            {
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile($"appsettings.{environment}.json", optional: true)
                    .Build();
                
                connectionString = configuration.GetConnectionString("DefaultConnection");
            }
            
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' not found and DATABASE_URL not set.");
            }

            Console.WriteLine($"[DbContextFactory] Environment: {environment}");
            Console.WriteLine($"[DbContextFactory] Connection string contains: Database={(connectionString.Contains("inkvault_dev") ? "inkvault_dev" : "OTHER")}");
            Console.WriteLine($"[DbContextFactory] Full connection (masked): {connectionString.Substring(0, Math.Min(100, connectionString.Length))}...");

            // For development, use connection string as-is (which has SslMode=Disable)
            // For production, ensure SSL is required
            if (environment.Equals("Development", StringComparison.OrdinalIgnoreCase))
            {
                // DEVELOPMENT: Use connection string exactly as configured (with SslMode=Disable)
                Console.WriteLine("[DbContextFactory] Using Development connection string with SslMode=Disable");
                optionsBuilder.UseNpgsql(connectionString);
            }
            else
            {
                // PRODUCTION: Parse and force SSL
                try 
                {
                    var builder = new NpgsqlConnectionStringBuilder();

                    if (connectionString.StartsWith("postgres://") || connectionString.StartsWith("postgresql://"))
                    {
                        var uri = new Uri(connectionString);
                        var userInfo = uri.UserInfo.Split(':');
                        
                        builder.Host = uri.Host;
                        builder.Port = uri.Port;
                        builder.Database = uri.AbsolutePath.TrimStart('/');
                        builder.Username = userInfo[0];
                        builder.Password = userInfo.Length > 1 ? userInfo[1] : "";
                    }
                    else
                    {
                        builder.ConnectionString = connectionString;
                    }

                    // Force required SSL for production
                    builder.SslMode = SslMode.Require;
                    
                    connectionString = builder.ToString();
                    Console.WriteLine("[DbContextFactory] Using Production connection string with SslMode=Require");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[DbContextFactory] Error parsing connection string: {ex.Message}");
                    // Use as-is if parsing fails
                }

                optionsBuilder.UseNpgsql(connectionString);
            }
            
            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}


