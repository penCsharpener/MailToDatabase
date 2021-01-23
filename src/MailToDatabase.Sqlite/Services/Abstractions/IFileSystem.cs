using System.Threading;
using System.Threading.Tasks;

namespace MailToDatabase.Sqlite.Services.Abstractions
{
    public interface IFileSystem
    {
        bool IsExported(string fileName);
        Task<bool> TryWriteAllBytesAsync(string fileName, byte[] content, CancellationToken cancellationToken = default);
    }
}
