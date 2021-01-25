using MailToDatabase.ImeReader.Models;
using MailToDatabase.ImeReader.Models.Configuration;
using MailToDatabase.ImeReader.Services.Abstractions;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MailToDatabase.ImeReader.Services
{
    public class WorkspaceProvider : IWorkspaceProvider
    {
        private readonly AppSettings _settings;

        public WorkspaceProvider(AppSettings settings)
        {
            _settings = settings;
        }

        public async IAsyncEnumerable<ImeFile> GetImeFilesAsync(CancellationToken cancellationToken = default)
        {
            switch (_settings.InputMode)
            {
                case InputModes.InputFile:
                    yield return await GetSingleImeFileAsync(cancellationToken);
                    break;
                case InputModes.InputDirectory:

                    break;
            }
        }

        private async Task<ImeFile> GetSingleImeFileAsync(CancellationToken cancellationToken = default)
        {
            var bytes = await File.ReadAllBytesAsync(_settings.ImeFilePath, cancellationToken);
            var imapMessage = await MailTypeConverter.DeserializeMimeMessageAsync(bytes, null);

            return new ImeFile()
            {
                FileInfo = new System.IO.FileInfo(_settings.ImeFilePath),
                ImapMessage = imapMessage
            };
        }
    }
}
