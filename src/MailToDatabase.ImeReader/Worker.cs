using MailToDatabase.ImeReader.Services.Abstractions;
using MailToDatabase.Shared.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace MailToDatabase.ImeReader
{
    public class Worker : BackgroundService
    {
        private readonly IWorkspaceProvider _workspaceProvider;
        private readonly ILogger<Worker> _logger;

        public Worker(IImeParser parser, IWorkspaceProvider workspaceProvider, ILogger<Worker> logger)
        {
            _workspaceProvider = workspaceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var ime in _workspaceProvider.GetImeFilesAsync(stoppingToken))
            {
                _logger.LogInformation(ime.ImapMessage.Subject);
            }
        }
    }
}
