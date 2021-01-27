using MailToDatabase.Shared.Models;
using System.Threading;
using System.Threading.Tasks;

namespace MailToDatabase.Shared.Services.Abstractions
{
    public interface IImeParser
    {
        Task ExtractAttachmentsAsync(ImeFile file, CancellationToken cancellationToken = default);
        Task ParseAsync(ImeFile file, CancellationToken cancellationToken = default);
    }
}
