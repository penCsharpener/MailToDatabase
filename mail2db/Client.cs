/*
MIT License

Copyright (c) 2019 Matthias Müller

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using MimeKit;
using penCsharpener.Mail2DB.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace penCsharpener.Mail2DB {
    public class Client : IDisposable, IMail2DBClient, IRetrievalClient {

        public string EmailAddress { get; }
        public string Password { get; }
        public string ServerURL { get; }
        public ushort Port { get; }
        public string OpenedMailFolder => _mailFolder?.Name;

        private const MessageSummaryItems MessageSummaryOptions = MessageSummaryItems.UniqueId
                                                              | MessageSummaryItems.Size
                                                              | MessageSummaryItems.Flags
                                                              //| MessageSummaryItems.BodyStructure
                                                              //| MessageSummaryItems.GMailLabels
                                                              //| MessageSummaryItems.GMailMessageId
                                                              //| MessageSummaryItems.InternalDate
                                                              //| MessageSummaryItems.PreviewText
                                                              | MessageSummaryItems.References
                                                              | MessageSummaryItems.Envelope;
        private ImapClient _imapClient = new ImapClient();
        private IMailFolder _mailFolder;
        public CancellationTokenSource cancel = new CancellationTokenSource();
        private string[] _mailFolders;
        private IList<UniqueId> _lastRetrievedUIds;

        public int MailCountInFolder => _mailFolder?.Count ?? -1;

        public Client(Credentials credentials) {
            EmailAddress = credentials.EmailAddress;
            ServerURL = credentials.ServerURL;
            Password = credentials.Password;
            Port = credentials.Port;
        }

        public Client(string usernameOrEmailAddress,
                      string serverURL,
                      string password,
                      ushort port) {
            EmailAddress = usernameOrEmailAddress;
            ServerURL = serverURL;
            Password = password;
            Port = port;
        }

        public async Task<int> GetTotalMailCount() {
            _mailFolder = await Authenticate();
            _mailFolder.Open(FolderAccess.ReadOnly);
            return (await _mailFolder.SearchAsync(SearchQuery.All))?.Count ?? 0;
        }

        public async Task<IList<string>> GetMailFolders(Action<Exception> errorHandeling = null) {
            try {
                if (!_imapClient.IsConnected) {
                    await _imapClient.ConnectAsync(ServerURL, Port, true);
                    _imapClient.AuthenticationMechanisms.Remove("XOAUTH");
                    await _imapClient.AuthenticateAsync(EmailAddress, Password);
                }
            } catch (Exception ex) {
                errorHandeling?.Invoke(ex);
            }
            var personal = _imapClient.GetFolder(_imapClient.PersonalNamespaces[0]);
            return personal.GetSubfolders(false, CancellationToken.None).Select(x => x.Name).ToList();
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
                    if (folder.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) {
                        return folder;
                    }
                }
            }
            _mailFolder = null;
            throw new MailFolderNotFoundException($"Mail folder(s) '{string.Join(", ", _mailFolders)}' not found.");
        }

        /// <summary>
        /// Sets the mailfolder the IMAP client should be running its methods on. 
        /// Search will check each variant of the folder names passed in and set
        /// the first that matches. Uses 'StringComparison.OrdinalIgnoreCase'.
        /// </summary>
        /// <param name="folderNames"></param>
        public void SetMailFolder(params string[] folderNames) {
            _mailFolders = folderNames;
        }

        public async Task<IList<UniqueId>> GetUIds(ImapFilter filter = null) {
            using (cancel = new CancellationTokenSource()) {

                _mailFolder = await Authenticate();
                await _mailFolder.OpenAsync(FolderAccess.ReadOnly);

                if (filter == null) {
                    _lastRetrievedUIds = await _mailFolder.SearchAsync(new SearchQuery());
                } else {
                    _lastRetrievedUIds = await _mailFolder.SearchAsync(filter.ToSearchQuery());
                }
                return _lastRetrievedUIds;
            }
        }

        public async Task<List<MimeMessageUId>> GetMessageUids(IList<UniqueId> uniqueIds) {
            using (cancel = new CancellationTokenSource()) {

                _mailFolder = await Authenticate();
                _mailFolder.Open(FolderAccess.ReadOnly);

                var list = new List<MimeMessageUId>();
                foreach (var uid in uniqueIds) {
                    list.Add(new MimeMessageUId(await _mailFolder.GetMessageAsync(uid), uid));
                }
                return list;
            }
        }

        public async Task<MimeMessageUId> GetMessageUid(UniqueId uniqueId) {
            using (cancel = new CancellationTokenSource()) {
                //_mailFolder = await Authenticate();
                //_mailFolder.Open(FolderAccess.ReadOnly);

                var mime = await _mailFolder.GetMessageAsync(uniqueId);
                if (mime != null) {
                    return new MimeMessageUId(mime, uniqueId);
                }
                return null;
            }
        }

        public async Task<MimeMessageUId> GetMessage(UniqueId uniqueId) {
            using (cancel = new CancellationTokenSource()) {
                _mailFolder = await Authenticate();
                await _mailFolder.OpenAsync(FolderAccess.ReadOnly);
                return new MimeMessageUId(await _mailFolder.GetMessageAsync(uniqueId), uniqueId);
            }
        }

        public async Task<IList<IMessageSummary>> GetSummaries() {
            var inbox = await Authenticate();
            await inbox.OpenAsync(FolderAccess.ReadOnly);
            return await inbox.FetchAsync(0, -1, MessageSummaryOptions);
        }

        public async Task<IList<IMessageSummary>> GetSummaries(IList<UniqueId> uids) {
            var inbox = await Authenticate();
            await inbox.OpenAsync(FolderAccess.ReadOnly);
            return await inbox.FetchAsync(uids, MessageSummaryOptions);
        }

        public async Task<IList<IMessageSummary>> GetSummaries(ImapFilter filter, uint[] uidsToExclude = null) {
            var inbox = await Authenticate();
            await inbox.OpenAsync(FolderAccess.ReadOnly);
            _lastRetrievedUIds = await GetUIds(filter);
            if (uidsToExclude?.Length > 0) {
                _lastRetrievedUIds = _lastRetrievedUIds.Where(x => !uidsToExclude.Contains(x.Id)).ToArray();
            }
            return await inbox.FetchAsync(_lastRetrievedUIds, MessageSummaryOptions);
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
            await DeleteMessages(uids);
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
            if (_mailFolder != null) {
                await _mailFolder.CloseAsync();
            }
            if (_imapClient?.IsConnected == true) {
                await _imapClient.DisconnectAsync(true);
            }
        }
    }
}
