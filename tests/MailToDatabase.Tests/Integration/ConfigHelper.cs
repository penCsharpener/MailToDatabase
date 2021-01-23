using MailToDatabase;
using MailToDatabase.Tests.Integration;
using Microsoft.Extensions.Configuration;

namespace UnitTests
{
    public static class ConfigHelper
    {
        public static Credentials GetCredentials()
        {
            return Startup.Configuration.GetSection(nameof(Credentials)).Get<Credentials>();
        }

        public static RetrievalConfiguration GetRetrievalConfiguration()
        {
            return Startup.Configuration.GetSection(nameof(RetrievalConfiguration)).Get<RetrievalConfiguration>();
        }
    }
}
