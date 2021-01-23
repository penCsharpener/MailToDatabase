using MailToDatabase.Contracts;
using MailToDatabase.Sqlite.Configuration;
using MailToDatabase.Sqlite.Extensions;
using MailToDatabase.Sqlite.Services.Abstractions;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace MailToDatabase.Sqlite.Services
{
    public class ImapService : IImapService
    {
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

        public async Task GetAllMails()
        {
            var uIds = await _retrievalClient.GetUIds(_settings.ToIntervalFilter());
            var existingMails = await _repo.GetExistingUidsAsync("inbox");
            var existingUids = existingMails.Select(x => x.UniqueId).ToList();
            var uIdFilter = new ImapFilter().Uids(uIds.Where(x => !existingUids.Contains(x.Id)).ToList());

            await foreach (var imapMessage in _mailConverter.GetMessagesAsync(uIdFilter))
            {
                var email = await _repo.SaveEmailAsync(imapMessage);

                if (email == null)
                {
                    _logger.LogInformation($"Email from imap folder '{imapMessage.MailFolder}', uId '{imapMessage.UId}' could not be saved to database.");
                    continue;
                }

                if (!await _fileSystem.TryWriteAllBytesAsync(email.FileName, imapMessage.MimeMessageBytes))
                {
                    await _repo.DeleteEmailAsync(email);
                    _logger.LogInformation($"Email from imap folder '{imapMessage.MailFolder}', uId '{imapMessage.UId}' could not be saved to disc.");
                }
            }
        }
    }
}
