using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MailToDatabase.ImeReader
{
    public class Startup
    {
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {

            services.AddHostedService<Worker>();
        }

        public static void ConfigureConfiguration(HostBuilderContext hostContext, IConfigurationBuilder builder)
        {

        }
    }
}
