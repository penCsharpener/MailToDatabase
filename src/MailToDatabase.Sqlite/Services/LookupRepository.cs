using MailToDatabase.Sqlite.Domain.Models;
using MailToDatabase.Sqlite.Domain.Records;
using MailToDatabase.Sqlite.Persistence;
using MailToDatabase.Sqlite.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MailToDatabase.Sqlite.Services
{
    public class LookupRepository : ILookupRepository
    {
        private readonly AppDbContext _dbContext;
        private readonly IHashProvider _hashProvider;

        public LookupRepository(AppDbContext dbContext, IFileSystem fileSystem, IHashProvider hashProvider)
        {
            _dbContext = dbContext;
            _hashProvider = hashProvider;
        }

        public async Task<bool> IsExportedAsync(uint uniqueId, Func<bool> fileSystemCheck = default)
        {
            var intId = Convert.ToInt32(uniqueId);
            var email = await _dbContext.Emails.FirstOrDefaultAsync(x => x.UniqueId == intId);

            if (email == null)
            {
                return false;
            }

            if (fileSystemCheck != null)
            {
                return fileSystemCheck.Invoke();
            }

            return true;
        }

        public async Task<IList<ExistingMail>> GetExistingUidsAsync(string mailFolder)
        {
            return await _dbContext.Emails.Where(x => x.FolderName.Equals(mailFolder, StringComparison.OrdinalIgnoreCase))
                .Select(x => new ExistingMail((uint)x.UniqueId, x.FileName))
                .ToListAsync();
        }

        public async Task<Email> SaveEmailAsync(ImapMessage imapMessage)
        {
            var sha = _hashProvider.ToSha256(imapMessage.MimeMessageBytes);
            var timestamp = imapMessage.ReceivedAtUTC.ToString("yyyyMMddHHmmss");
            var filename = $"{timestamp}_{imapMessage.From.EmailAddress}_{sha.Substring(0, 16)}";

            var attachments = imapMessage.Attachments.Select(x => new Attachment
            {
                FileName = x.Filename,
            }).ToList();

            var email = new Email
            {
                Body = imapMessage.Body,
                Subject = imapMessage.Subject,
                AttachmentCount = imapMessage.Attachments.Length,
                Sha256 = sha,
                FileName = filename,
                FolderName = imapMessage.MailFolder,
                FromAddress = imapMessage.From.EmailAddress,
                ReceivedAt = imapMessage.ReceivedAtUTC,
                UniqueId = Convert.ToInt32(imapMessage.UId),
                Attachments = attachments
            };

            _dbContext.Emails.Add(email);
            await _dbContext.SaveChangesAsync();

            return email;
        }

        public async Task DeleteEmailAsync(Email email)
        {
            if (email.Attachments != null && email.Attachments.Count > 0)
            {
                _dbContext.Attachments.RemoveRange(email.Attachments);
            }

            _dbContext.Emails.Remove(email);

            await _dbContext.SaveChangesAsync();
        }
    }
}
