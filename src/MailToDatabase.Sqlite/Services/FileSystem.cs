using MailToDatabase.Sqlite.Configuration;
using MailToDatabase.Sqlite.Extensions;
using MailToDatabase.Sqlite.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MailToDatabase.Sqlite.Services
{
    public class FileSystem : IFileSystem
    {
        private const string EXPORT_PATH = "export";
        private AppSettings _appSettings;
        private readonly ILogger<FileSystem> _logger;
        private string _mailFolderPath;

        public FileSystem(AppSettings settings, ILogger<FileSystem> logger)
        {
            _appSettings = settings;
            _mailFolderPath = settings.MailFolderName.Replace(" ", "_").Replace(Path.GetInvalidPathChars(), null).ToLower();
            _logger = logger;
        }

        public async Task<bool> TryWriteAllBytesAsync(string fileName, byte[] content, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullPath = Path.Combine(_appSettings.DownloadDirectory, EXPORT_PATH, _mailFolderPath, fileName + ".ime");

                CheckAndCreateDirectories();

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

        private void CheckAndCreateDirectories()
        {
            var dir = Path.Combine(_appSettings.DownloadDirectory, EXPORT_PATH, _mailFolderPath);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
