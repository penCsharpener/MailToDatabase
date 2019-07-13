using MailKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace penCsharpener.Mail2DB.Contracts {
    public interface IRetrievalClient {
        string OpenedMailFolder { get; }
        Task<IList<IMessageSummary>> GetSummaries();
        Task<IList<IMessageSummary>> GetSummaries(IList<UniqueId> uids);
        Task<IList<string>> GetMailFolders(Action<Exception> errorHandeling = null);
        Task<MimeMessageUId> GetMessage(UniqueId uniqueId);
        Task<MimeMessageUId> GetMessageUid(UniqueId uniqueId);
        Task<List<MimeMessageUId>> GetMessageUids(IList<UniqueId> uniqueIds);
        Task<int> GetTotalMailCount();
        Task<IList<UniqueId>> GetUIds(ImapFilter filter = null);
        void SetMailFolder(params string[] folderNames);
    }
}
