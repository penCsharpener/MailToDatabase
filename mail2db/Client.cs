using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace penCsharpener.Mail2DB {
    public class Client : IDisposable {

        public string EmailAddress { get; set; }
        public string Password { get; set; }
        public string ServerURL { get; set; }
        public ushort Port { get; set; }

        private ImapClient _imapClient = new ImapClient();
        private CancellationTokenSource cancel = new CancellationTokenSource();

        public Client(Credentials credentials) {
            EmailAddress = credentials.EmailAddress;
            ServerURL = credentials.ServerURL;
            Password = credentials.Password;
            Port = credentials.Port;
        }

        private async Task<IMailFolder> Authenticate(CancellationTokenSource cancel, Action<Exception> errorHandeling = null) {
            try {
                if (!_imapClient.IsConnected) {
                    await _imapClient.ConnectAsync(ServerURL, Port, true, cancel.Token);
                    _imapClient.AuthenticationMechanisms.Remove("XOAUTH");
                    await _imapClient.AuthenticateAsync(EmailAddress, Password, cancel.Token);
                }
            } catch (Exception ex) {
                errorHandeling?.Invoke(ex);
            }
            return _imapClient.Inbox;
        }

        public async Task<IList<UniqueId>> GetUIds(ImapFilter filter = null) {
            using (cancel = new CancellationTokenSource()) {

                var inbox = await Authenticate(cancel);
                await inbox.OpenAsync(FolderAccess.ReadOnly, cancel.Token);

                if (filter == null) {
                    return await inbox.SearchAsync(new SearchQuery(), cancel.Token);
                } else {
                    return await inbox.SearchAsync(filter.ToSearchQuery(), cancel.Token);
                }
            }
        }

        public async Task<List<MimeMessageUId>> GetMessages(IList<UniqueId> uniqueIds) {
            using (cancel = new CancellationTokenSource()) {

                var inbox = await Authenticate(cancel);
                inbox.Open(FolderAccess.ReadOnly, cancel.Token);

                var list = new List<MimeMessageUId>();
                foreach (var uid in uniqueIds) {
                    list.Add(new MimeMessageUId(inbox.GetMessage(uid, cancel.Token), uid));
                }
                return list;
            }
        }

        public async Task<MimeMessage> GetMessage(UniqueId uniqueId) {
            using (cancel = new CancellationTokenSource()) {
                var inbox = await Authenticate(cancel);
                await inbox.OpenAsync(FolderAccess.ReadOnly, cancel.Token);
                return await inbox.GetMessageAsync(uniqueId, cancel.Token);
            }
        }

        public async Task<IList<IMessageSummary>> GetSummary() {
            var inbox = await Authenticate(cancel);
            await inbox.OpenAsync(FolderAccess.ReadOnly, cancel.Token);
            return await inbox.FetchAsync(0, -1, MessageSummaryItems.UniqueId
                                               | MessageSummaryItems.Size
                                               | MessageSummaryItems.Flags
                                               | MessageSummaryItems.BodyStructure
                                               | MessageSummaryItems.GMailLabels
                                               | MessageSummaryItems.GMailMessageId
                                               | MessageSummaryItems.InternalDate
                                               | MessageSummaryItems.PreviewText
                                               | MessageSummaryItems.References
                                               | MessageSummaryItems.Envelope);
        }

        public async void Dispose() {
            await _imapClient.DisconnectAsync(true);
        }
    }
}
