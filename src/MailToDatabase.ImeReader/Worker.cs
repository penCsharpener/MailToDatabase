using MailToDatabase.ImeReader.Services.Abstractions;
using MailToDatabase.Shared.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace MailToDatabase.ImeReader
{
    public class Worker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IWorkspaceProvider _workspaceProvider;
        private readonly ILogger<Worker> _logger;

        public Worker(IServiceScopeFactory scopeFactory, IWorkspaceProvider workspaceProvider, ILogger<Worker> logger)
        {
            _scopeFactory = scopeFactory;
            _workspaceProvider = workspaceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var ime in _workspaceProvider.GetImeFilesAsync(stoppingToken))
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var parser = scope.ServiceProvider.GetRequiredService<IImeParser>();

                    await parser.ParseAsync(ime);
                }
            }
        }
    }
}
