using MailToDatabase.Sqlite.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Threading.Tasks;

namespace MailToDatabase.Sqlite
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var config = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json", false, true)
                    .AddUserSecrets<Program>(true, true)
                    .Build();

            Log.Logger = new LoggerConfiguration().ReadFrom.Configuration(config).CreateLogger();

            try
            {
                var host = CreateHostBuilder(args).Build();

                using (var scope = host.Services.CreateScope())
                {
                    var sp = scope.ServiceProvider;

                    try
                    {
                        await sp.MigrateDatabaseAsync();
                    }
                    catch (Exception ex)
                    {
                        var logger = sp.GetRequiredService<ILogger<Program>>();
                        logger.LogError(ex, "An error occurred while migrating the database.");
                    }
                }

                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly.");
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .ConfigureServices((hostContext, services) => services.RegisterServices(hostContext))
                       .UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));
        }
    }
}
