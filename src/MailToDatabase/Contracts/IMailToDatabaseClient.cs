using MailKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MailToDatabase.Contracts
{
    /// <summary>
    /// state changing methods that alter Imap account content
    /// </summary>
    public interface IMail2DBClient
    {
        Task<IMailFolder> Authenticate(Action<Exception> errorHandeling = null);
        Task ExpungeMail();
        Task MarkAsRead(IList<UniqueId> uids);
        Task DeleteMessages(IList<UniqueId> uids);
        Task DeleteMessages(ImapFilter imapFilter);
        Task<UniqueId?> AppendMessage(MimeMessage mimeMessage, bool asNotSeen = false);
    }
}