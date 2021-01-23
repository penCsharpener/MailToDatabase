using MailToDatabase.Contracts;
using MailToDatabase.Sqlite.Configuration;
using MailToDatabase.Sqlite.Services;
using MailToDatabase.Sqlite.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MailToDatabase.Sqlite
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton<IFileSystem, FileSystem>()
                    .AddTransient<ILookupRepository, LookupRepository>()
                    .AddTransient<IImapService, ImapService>()
                    .AddSingleton<IHashProvider, HashProvider>()
                    .AddSingleton(_ => hostContext.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>())
                    .AddTransient<IRetrievalClient>(sp => new Client(hostContext.Configuration.GetSection(nameof(Credentials)).Get<Credentials>()))
                    .AddHostedService<Worker>();
                });
    }
}
