using MailToDatabase.Sqlite.Configuration;
using MailToDatabase.Sqlite.Services.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MailToDatabase.Sqlite.Services
{
    public class FileSystem : IFileSystem
    {
        private AppSettings _appSettings;
        private readonly ILogger<FileSystem> _logger;

        public FileSystem(IConfiguration configuration, ILogger<FileSystem> logger)
        {
            _appSettings = configuration.GetSection(nameof(AppSettings)).Get<AppSettings>();
            _logger = logger;
        }

        public async Task<bool> TryWriteAllBytesAsync(string fileName, byte[] content, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullPath = Path.Combine(_appSettings.DownloadDirectory, fileName + ".ime");

                if (!File.Exists(fullPath))
                {
                    await File.WriteAllBytesAsync(fullPath, content, cancellationToken);
                    return true;
                }
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
            return false;
        }

        public bool IsExported(string fileName)
        {
            var fullPath = Path.Combine(_appSettings.DownloadDirectory, fileName, ".ime");

            return File.Exists(fullPath);
        }
    }
}
