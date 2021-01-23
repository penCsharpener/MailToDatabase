using Microsoft.Extensions.Configuration;

namespace MailToDatabase.Tests.Integration
{
    public class Startup
    {
        public static IConfiguration Configuration { get; }

        static Startup()
        {
            Configuration = new ConfigurationBuilder()
                .AddUserSecrets<Startup>()
                .Build();
        }
    }
}
