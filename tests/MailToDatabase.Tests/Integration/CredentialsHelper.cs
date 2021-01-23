using MailToDatabase.Tests.Integration;
using penCsharpener.Mail2DB;
using System;

namespace UnitTests
{
    public static class CredentialsHelper
    {
        public static Credentials GetCredentials()
        {
            return new Credentials
            {
                EmailAddress = Startup.Configuration["Credentials:EmailAddress"],
                Password = Startup.Configuration["Credentials:Password"],
                ServerURL = Startup.Configuration["Credentials:ServerURL"],
                Port = (ushort)Convert.ToUInt32(Startup.Configuration["Credentials:Port"])
            };
        }
    }
}
