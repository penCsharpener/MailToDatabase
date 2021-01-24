using MailToDatabase.Clients;
using MailToDatabase.Contracts;
using MailToDatabase.Sqlite.Configuration;
using MailToDatabase.Sqlite.Persistence;
using MailToDatabase.Sqlite.Services;
using MailToDatabase.Sqlite.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;

namespace MailToDatabase.Sqlite.Extensions
{
    public static class ServiceExtensions
    {
        public static async Task MigrateDatabaseAsync(this IServiceProvider sp)
        {
            await sp.GetService<AppDbContext>().Database.MigrateAsync();
        }

        public static IServiceCollection RegisterServices(this IServiceCollection services, HostBuilderContext hostContext)
        {
            services.AddDbContext<AppDbContext>(options => options.UseSqlite(hostContext.Configuration.GetConnectionString("DefaultConnection")));

            services
            .AddSingleton<IFileSystem, FileSystem>()
            .AddTransient<ILookupRepository, LookupRepository>()
            .AddTransient<IImapService, ImapService>()
            .AddSingleton<IHashProvider, HashProvider>()
            .AddSingleton(_ => hostContext.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>())
            .AddTransient<IRetrievalClient>(sp => new GenericClient(hostContext.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>().Credentials))
            .AddHostedService<Worker>();

            return services;
        }
    }
}
