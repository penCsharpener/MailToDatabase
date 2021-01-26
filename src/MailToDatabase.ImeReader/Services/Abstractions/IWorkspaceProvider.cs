using MailToDatabase.Shared.Models;
using System.Collections.Generic;
using System.Threading;

namespace MailToDatabase.ImeReader.Services.Abstractions
{
    public interface IWorkspaceProvider
    {
        IAsyncEnumerable<ImeFile> GetImeFilesAsync(CancellationToken cancellationToken = default);
    }
}
