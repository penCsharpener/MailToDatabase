using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Threading;

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

        private IMailFolder Authenticate(CancellationTokenSource cancel, Action<Exception> errorHandeling = null) {
            try {
                if (!_imapClient.IsConnected) {
                    _imapClient.Connect(ServerURL, Port, true, cancel.Token);
                    _imapClient.AuthenticationMechanisms.Remove("XOAUTH");
                    _imapClient.Authenticate(EmailAddress, Password, cancel.Token);
                }
            } catch (Exception ex) {
                errorHandeling?.Invoke(ex);
            }
            return _imapClient.Inbox;
        }

        public IList<UniqueId> GetUIds() {
            using (cancel = new CancellationTokenSource()) {

                var inbox = Authenticate(cancel);
                inbox.Open(FolderAccess.ReadOnly, cancel.Token);

                var query = SearchQuery.DeliveredAfter(DateTime.Now.AddDays(-30));
                var uids = inbox.Search(query, cancel.Token);

                return uids;
            }
        }

        public IEnumerable<MimeMessage> GetMessages(IList<UniqueId> uniqueIds) {
            using (cancel = new CancellationTokenSource()) {

                var inbox = Authenticate(cancel);
                inbox.Open(FolderAccess.ReadOnly, cancel.Token);

                foreach (var uid in uniqueIds) {
                    yield return inbox.GetMessage(uid, cancel.Token);
                }
            }
        }

        public MimeMessage GetMessage(UniqueId uniqueId) {
            using (cancel = new CancellationTokenSource()) {
                var inbox = Authenticate(cancel);
                inbox.Open(FolderAccess.ReadOnly, cancel.Token);
                return inbox.GetMessage(uniqueId, cancel.Token);
            }
        }

        public IList<IMessageSummary> GetSummary() {
            var inbox = Authenticate(cancel);
            inbox.Open(FolderAccess.ReadOnly, cancel.Token);
            return inbox.Fetch(0, -1, MessageSummaryItems.UniqueId
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

        public void Dispose() {
            _imapClient.Disconnect(true);
        }
    }
}
