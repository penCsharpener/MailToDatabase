using MailToDatabase.Contracts;
using MailToDatabase.Sqlite.Configuration;
using MailToDatabase.Sqlite.Extensions;
using MailToDatabase.Sqlite.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MailToDatabase.Sqlite.Services
{
    public class ImapService : IImapService
    {
        private const string EXPORT_PATH = "export";
        /// <summary>
        /// ime = Imap Mail Export
        /// </summary>
        private const string IME_FILE_EXTENSION = ".ime";
        private readonly MailTypeConverter _mailConverter;
        private readonly IRetrievalClient _retrievalClient;
        private readonly ILookupRepository _repo;
        private readonly IFileSystem _fileSystem;
        private readonly AppSettings _settings;
        private readonly ILogger<ImapService> _logger;

        public ImapService(IRetrievalClient retrievalClient, ILookupRepository repo, IFileSystem fileSystem, AppSettings settings, ILogger<ImapService> logger)
        {
            _mailConverter = new MailTypeConverter(retrievalClient);
            _retrievalClient = retrievalClient;
            _repo = repo;
            _fileSystem = fileSystem;
            _settings = settings;
            _logger = logger;
        }

        public async Task GetAllMailsAsync()
        {
            _retrievalClient.SetMailFolder(_settings.MailFolderName);
            var uIds = await _retrievalClient.GetUIds(new ImapFilter());
            var existingMails = await _repo.GetExistingUidsAsync(_settings.MailFolderName);
            var existingUids = existingMails.Select(x => x.UniqueId).ToList();
            ImapFilter uIdFilter = new();

            if (existingUids.Count > 0)
            {
                uIdFilter = new ImapFilter().Uids(uIds.Where(x => !existingUids.Contains(x.Id)).ToList());
            }

            await foreach (var imapMessage in _mailConverter.GetMessagesAsync(uIdFilter))
            {
                try
                {
                    var email = await _repo.SaveEmailAsync(imapMessage);

                    if (email == null)
                    {
                        _logger.LogInformation($"Email from imap folder '{imapMessage.MailFolder}', uId '{imapMessage.UId}' could not be saved to database.");
                        continue;
                    }

                    var fullFileName = Path.Combine(_settings.DownloadDirectory, EXPORT_PATH, CleanUpMailFolderName(_settings.MailFolderName), email.FileName + IME_FILE_EXTENSION);

                    if (!await _fileSystem.TryWriteAllBytesAsync(fullFileName, imapMessage.MimeMessageBytes))
                    {
                        await _repo.DeleteEmailAsync(email);
                        _logger.LogInformation($"Email from imap folder '{imapMessage.MailFolder}', uId '{imapMessage.UId}' could not be saved to disc.");
                    }
                }
                catch (System.Exception ex)
                {
                    _logger.LogError(ex.Message);
                }
            }
        }

        private string CleanUpMailFolderName(string mailFolderName)
        {
            return mailFolderName.Replace(" ", "_").Replace(Path.GetInvalidPathChars(), null).ToLower();
        }
    }
}
