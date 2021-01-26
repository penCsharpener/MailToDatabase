using Microsoft.Extensions.Hosting;
using Serilog;

namespace MailToDatabase.ImeReader
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                       .ConfigureAppConfiguration(Startup.ConfigureConfiguration(args))
                       .ConfigureServices(Startup.ConfigureServices)
                       .UseSerilog((context, loggerConfig) => loggerConfig.ReadFrom.Configuration(context.Configuration));
        }
    }
}
