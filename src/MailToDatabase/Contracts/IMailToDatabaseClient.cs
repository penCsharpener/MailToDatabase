using MailKit;
using MimeKit;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace MailToDatabase.Contracts
{
    /// <summary>
    /// state changing methods that alter Imap account content
    /// </summary>
    public interface IMailToDatabaseClient
    {
        Task<IMailFolder> AuthenticateAsync(CancellationToken cancellationToken = default);
        Task ExpungeMail();
        Task MarkAsRead(IList<UniqueId> uids);
        Task DeleteMessages(IList<UniqueId> uids);
        Task DeleteMessages(ImapFilter imapFilter);
        Task<UniqueId?> AppendMessage(MimeMessage mimeMessage, bool asNotSeen = false);
    }
}