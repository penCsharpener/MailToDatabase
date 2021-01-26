using MailToDatabase.ImeReader.Models.Configuration;
using MailToDatabase.ImeReader.Services;
using MailToDatabase.ImeReader.Services.Abstractions;
using MailToDatabase.Shared.Services;
using MailToDatabase.Shared.Services.Abstractions;
using MailToDatabase.Sqlite.Services;
using MailToDatabase.Sqlite.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MailToDatabase.ImeReader
{
    public static class Startup
    {
        public static void ConfigureServices(HostBuilderContext hostContext, IServiceCollection services)
        {
            var settings = hostContext.Configuration.GetSection(nameof(AppSettings)).Get<AppSettings>() ?? new();
            settings.ParseCommandlineParameters(hostContext.Configuration);

            services.AddSingleton(settings)
                    .AddTransient<IWorkspaceProvider, WorkspaceProvider>()
                    .AddTransient<IFileSystem, FileSystem>()
                    .AddTransient<IImeParser, ImeParser>();

            services.AddHostedService<Worker>();
        }

        public static Action<HostBuilderContext, IConfigurationBuilder> ConfigureConfiguration(string[] args)
        {
            var switchMapping = new Dictionary<string, string> {
                { "--path", "InputPath" },
                { "--output", "OutputFolderPath" }
            };

            return (hostContext, builder) =>
            {
                builder.Sources.Clear();
                builder.SetBasePath(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                       .AddJsonFile("appsettings.json", true, true)
                       .AddUserSecrets<Program>(true, true)
                       .AddEnvironmentVariables()
                       .AddCommandLine(args, switchMapping);
            };
        }
    }
}
