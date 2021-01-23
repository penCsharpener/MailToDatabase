using MailToDatabase.Sqlite.Domain.Models;
using MailToDatabase.Sqlite.Domain.Records;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MailToDatabase.Sqlite.Services.Abstractions
{
    public interface ILookupRepository
    {
        Task DeleteEmailAsync(Email email);
        Task<IList<ExistingMail>> GetExistingUidsAsync(string mailFolder);
        Task<bool> IsExportedAsync(uint uniqueId, Func<bool> fileSystemCheck = default);
        Task<Email> SaveEmailAsync(ImapMessage imapMessage);
    }
}
