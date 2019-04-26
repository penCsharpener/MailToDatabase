using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace penCsharpener.Mail2DB {
    public class Client : IDisposable {

        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string ServerURL { get; set; }
        public ushort Port { get; set; }
        public string OpenedMailFolder => _mailFolder?.Name;

        private ImapClient _imapClient = new ImapClient();
        private IMailFolder _mailFolder;
        public CancellationTokenSource cancel = new CancellationTokenSource();
        private string[] _mailFolders;

        public int MailCountInFolder => _mailFolder?.Count ?? -1;

        public Client(Credentials credentials) {
            EmailAddress = credentials.EmailAddress;
            ServerURL = credentials.ServerURL;
            Password = credentials.Password;
            Port = credentials.Port;
        }

        public async Task<IMailFolder> Authenticate(Action<Exception> errorHandeling = null) {
            try {
                if (!_imapClient.IsConnected) {
                    await _imapClient.ConnectAsync(ServerURL, Port, true);
                    _imapClient.AuthenticationMechanisms.Remove("XOAUTH");
                    await _imapClient.AuthenticateAsync(EmailAddress, Password);
                }
            } catch (Exception ex) {
                errorHandeling?.Invoke(ex);
            }
            if (_mailFolders == null) {
                _mailFolders = new[] { "inbox" };
            }

            var personal = _imapClient.GetFolder(_imapClient.PersonalNamespaces[0]);
            foreach (var folder in personal.GetSubfolders(false, CancellationToken.None)) {
                foreach (var name in _mailFolders) {
                    if (folder.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0) {
                        _mailFolder = folder;
                        return folder;
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Sets the mailfolder the IMAP client should be running its methods on. 
        /// Search will check each variant of the folder names passed in and set
        /// the first that matches. Uses 'StringComparison.OrdinalIgnoreCase'.
        /// </summary>
        /// <param name="folderNames"></param>
        public void SetMailFolder(params string[] folderNames) => _mailFolders = folderNames;

        public async Task<IList<UniqueId>> GetUIds(ImapFilter filter = null) {
            using (cancel = new CancellationTokenSource()) {

                _mailFolder ??= await Authenticate();
                await _mailFolder.OpenAsync(FolderAccess.ReadOnly);

                if (filter == null) {
                    return await _mailFolder.SearchAsync(new SearchQuery());
                } else {
                    return await _mailFolder.SearchAsync(filter.ToSearchQuery());
                }
            }
        }

        public async Task<List<MimeMessageUId>> GetMessages(IList<UniqueId> uniqueIds) {
            using (cancel = new CancellationTokenSource()) {

                _mailFolder ??= await Authenticate();
                _mailFolder.Open(FolderAccess.ReadOnly);

                var list = new List<MimeMessageUId>();
                foreach (var uid in uniqueIds) {
                    list.Add(new MimeMessageUId(_mailFolder.GetMessage(uid), uid));
                }
                return list;
            }
        }

        public async Task<MimeMessage> GetMessage(UniqueId uniqueId) {
            using (cancel = new CancellationTokenSource()) {
                _mailFolder ??= await Authenticate();
                await _mailFolder.OpenAsync(FolderAccess.ReadOnly);
                return await _mailFolder.GetMessageAsync(uniqueId);
            }
        }

        public async Task<IList<IMessageSummary>> GetAllSummaries() {
            var inbox = await Authenticate();
            await inbox.OpenAsync(FolderAccess.ReadOnly);
            return await inbox.FetchAsync(0, -1, MessageSummaryItems.UniqueId
                                               | MessageSummaryItems.Size
                                               | MessageSummaryItems.Flags
                                               //| MessageSummaryItems.BodyStructure
                                               //| MessageSummaryItems.GMailLabels
                                               //| MessageSummaryItems.GMailMessageId
                                               //| MessageSummaryItems.InternalDate
                                               //| MessageSummaryItems.PreviewText
                                               | MessageSummaryItems.References
                                               | MessageSummaryItems.Envelope);
        }

        public async Task<IList<IMessageSummary>> GetSummaries(IList<UniqueId> uids) {
            var inbox = await Authenticate();
            await inbox.OpenAsync(FolderAccess.ReadOnly);
            return await inbox.FetchAsync(uids, MessageSummaryItems.UniqueId
                                               | MessageSummaryItems.Size
                                               | MessageSummaryItems.Flags
                                               //| MessageSummaryItems.BodyStructure
                                               //| MessageSummaryItems.GMailLabels
                                               //| MessageSummaryItems.GMailMessageId
                                               //| MessageSummaryItems.InternalDate
                                               //| MessageSummaryItems.PreviewText
                                               | MessageSummaryItems.References
                                               | MessageSummaryItems.Envelope);
        }

        public async Task MarkAsRead(IList<UniqueId> uids) {
            await _mailFolder.OpenAsync(FolderAccess.ReadWrite);
            await _mailFolder.SetFlagsAsync(uids, MessageFlags.Seen, false);
        }

        public async Task DeleteMessages(IList<UniqueId> uids) {
            await _mailFolder.OpenAsync(FolderAccess.ReadWrite);
            await _mailFolder.SetFlagsAsync(uids, MessageFlags.Deleted, false);
        }

        public async Task DeleteMessages(ImapFilter imapFilter) {
            var uids = await GetUIds(imapFilter);
            var sums = await GetSummaries(uids);
            await _mailFolder.OpenAsync(FolderAccess.ReadWrite);
            await _mailFolder.SetFlagsAsync(uids, MessageFlags.Deleted, false);
        }

        public async Task ExpungeMail() {
            await _mailFolder.OpenAsync(FolderAccess.ReadWrite);
            await _mailFolder.ExpungeAsync();
        }

        public async Task<UniqueId?> AppendMessage(MimeMessage mimeMessage, bool asNotSeen = false) {
            var flags = asNotSeen ? MessageFlags.None : MessageFlags.Seen;
            await _mailFolder.OpenAsync(FolderAccess.ReadWrite);
            return await _mailFolder.AppendAsync(mimeMessage);
        }

        public async void Dispose() {
            await _imapClient.DisconnectAsync(true);
        }
    }
}
