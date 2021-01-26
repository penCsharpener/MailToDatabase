using System.Threading;
using System.Threading.Tasks;

namespace MailToDatabase.Sqlite.Services.Abstractions
{
    public interface IFileSystem
    {
        bool IsExported(string fileName);
        Task<string> ReadAllTextAsync(string fileName, CancellationToken cancellationToken = default);
        Task<bool> TryWriteAllBytesAsync(string fileName, byte[] content, CancellationToken cancellationToken = default);
    }
}
