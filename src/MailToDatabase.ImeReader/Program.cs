using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

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
            var switchMapping = new Dictionary<string, string> {
                { "--path", "InputPath" },
                { "--output", "OutputFolderPath" }
            };

            return Host.CreateDefaultBuilder(args)
                       .ConfigureAppConfiguration((context, builder) =>
                       {
                           builder.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                           .AddCommandLine(args, switchMapping)
                           .AddUserSecrets<Program>()
                           .AddEnvironmentVariables();
                       })
                       .ConfigureServices(Startup.ConfigureServices);
        }
    }
}
