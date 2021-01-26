using MailToDatabase.Sqlite.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MailToDatabase.Sqlite.Services
{
    public class FileSystem : IFileSystem
    {
        private readonly ILogger<FileSystem> _logger;

        public FileSystem(ILogger<FileSystem> logger)
        {
            _logger = logger;
        }

        public async Task<bool> TryWriteAllBytesAsync(string fileName, byte[] content, CancellationToken cancellationToken = default)
        {
            try
            {
                var fullPath = Path.Combine(fileName);

                CheckAndCreateDirectories(fileName);

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
            var fullPath = Path.Combine(fileName);

            return File.Exists(fullPath);
        }

        public async Task<string> ReadAllTextAsync(string fileName, CancellationToken cancellationToken = default)
        {
            return await File.ReadAllTextAsync(fileName, cancellationToken);
        }

        private void CheckAndCreateDirectories(string fileName)
        {
            var dir = Path.GetDirectoryName(fileName);

            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }
    }
}
