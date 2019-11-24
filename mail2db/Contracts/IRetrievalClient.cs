using MailKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace penCsharpener.Mail2DB.Contracts {
    /// <summary>
    /// All methods are not changing state of the imap account
    /// </summary>
    public interface IRetrievalClient : IDisposable {
        string OpenedMailFolder { get; }
        Task<IList<IMessageSummary>> GetSummaries();
        Task<IList<IMessageSummary>> GetSummaries(IList<UniqueId> uids);
        Task<IList<IMessageSummary>> GetSummaries(ImapFilter filter, uint[] uidsToExclude = null);
        Task<IList<string>> GetMailFolders(Action<Exception> errorHandeling = null);
        Task<MimeMessageUId> GetMessage(UniqueId uniqueId);
        Task<MimeMessageUId> GetMessageUid(UniqueId uniqueId);
        Task<List<MimeMessageUId>> GetMessageUids(IList<UniqueId> uniqueIds);
        Task<int> GetTotalMailCount();
        Task<IList<UniqueId>> GetUIds(ImapFilter filter = null);
        void SetMailFolder(params string[] folderNames);
    }
}
