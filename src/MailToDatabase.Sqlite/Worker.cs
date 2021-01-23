using MailToDatabase.Sqlite.Configuration;
using MailToDatabase.Sqlite.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace MailToDatabase.Sqlite
{
    public class Worker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly AppSettings _settings;
        private readonly ILogger<Worker> _logger;

        public Worker(IServiceScopeFactory scopeFactory, AppSettings settings, ILogger<Worker> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = settings;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _scopeFactory.CreateScope())
                {
                    var imapService = scope.ServiceProvider.GetRequiredService<IImapService>();

                    await imapService.GetAllMails();
                }
                await Task.Delay(_settings.RetrievalIntervalMinutes * 60 * 1000, stoppingToken);
            }
        }
    }
}
