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
using MailToDatabase.Contracts;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MailToDatabase.Clients
{
    public class GenericClient : IMailToDatabaseClient, IRetrievalClient
    {
        public string EmailAddress { get; }
        public string Password { get; }
        public string ServerURL { get; }
        public ushort Port { get; }
        public string OpenedMailFolder => _mailFolder?.Name;

        protected const MessageSummaryItems MessageSummaryOptions = MessageSummaryItems.UniqueId
                                                              | MessageSummaryItems.Size
                                                              | MessageSummaryItems.Flags
                                                              //| MessageSummaryItems.BodyStructure
                                                              //| MessageSummaryItems.GMailLabels
                                                              //| MessageSummaryItems.GMailMessageId
                                                              //| MessageSummaryItems.InternalDate
                                                              //| MessageSummaryItems.PreviewText
                                                              | MessageSummaryItems.References
                                                              | MessageSummaryItems.Envelope;

        protected readonly ImapClient _imapClient = new ImapClient();
        protected IMailFolder _mailFolder;
        protected string[] _mailFolders;
        protected IList<UniqueId> _lastRetrievedUIds;

        public int MailCountInFolder => _mailFolder?.Count ?? -1;

        public GenericClient(Credentials credentials)
        {
            EmailAddress = credentials.EmailAddress;
            ServerURL = credentials.ServerURL;
            Password = credentials.Password;
            Port = credentials.Port;
        }

        public GenericClient(string usernameOrEmailAddress,
                      string serverURL,
                      string password,
                      ushort port)
        {
            EmailAddress = usernameOrEmailAddress;
            ServerURL = serverURL;
            Password = password;
            Port = port;
        }

        public virtual async Task<int> GetTotalMailCount()
        {
            _mailFolder = await AuthenticateAsync();
            _mailFolder.Open(FolderAccess.ReadOnly);
            return (await _mailFolder.SearchAsync(SearchQuery.All))?.Count ?? 0;
        }

        public virtual async Task<IList<string>> GetMailFoldersAsync(Action<Exception> errorHandeling = null)
        {
            try
            {
                if (!_imapClient.IsConnected)
                {
                    await _imapClient.ConnectAsync(ServerURL, Port, true);
                    _imapClient.AuthenticationMechanisms.Remove("XOAUTH");
                    await _imapClient.AuthenticateAsync(EmailAddress, Password);
                }
            }
            catch (Exception ex)
            {
                errorHandeling?.Invoke(ex);
            }
            var personal = _imapClient.GetFolder(_imapClient.PersonalNamespaces[0]);

            var list = new List<string>();
            await foreach (var folder in GetFolderRecursivelyAsync(personal))
            {
                list.Add(folder.Name);
            }
            return list;
        }

        protected virtual async IAsyncEnumerable<IMailFolder> GetFolderRecursivelyAsync(IMailFolder mailFolder)
        {
            foreach (var subFolder in await mailFolder.GetSubfoldersAsync(false, CancellationToken.None))
            {
                if ((await subFolder.GetSubfoldersAsync(false, CancellationToken.None)).Any())
                {
                    await foreach (var folder in GetFolderRecursivelyAsync(subFolder))
                    {
                        yield return folder;
                    }
                }

                yield return subFolder;
            }
        }

        public virtual async Task<IMailFolder> AuthenticateAsync(CancellationToken cancellationToken = default)
        {
            if (!_imapClient.IsConnected)
            {
                await _imapClient.ConnectAsync(ServerURL, Port, true);
                _imapClient.AuthenticationMechanisms.Remove("XOAUTH");
                await _imapClient.AuthenticateAsync(EmailAddress, Password);
            }

            if (_mailFolders == null)
            {
                _mailFolders = new[] { "inbox" };
            }

            var personal = _imapClient.GetFolder(_imapClient.PersonalNamespaces[0]);

            await foreach (var folder in GetFolderRecursivelyAsync(personal))
            {
                if (_mailFolders.Contains(folder.Name.ToLower()))
                {
                    return folder;
                }
            }

            throw new MailFolderNotFoundException($"Mail folder(s) '{string.Join(", ", _mailFolders)}' not found.");
        }

        /// <summary>
        /// Sets the mailfolder the IMAP client should be running its methods on.
        /// Search will check each variant of the folder names passed in and set
        /// the first that matches. Uses 'StringComparison.OrdinalIgnoreCase'.
        /// </summary>
        /// <param name="folderNames"></param>
        public virtual void SetMailFolder(params string[] folderNames)
        {
            _mailFolders = folderNames.Select(x => x.ToLower()).ToArray();
        }

        public virtual async Task<IList<UniqueId>> GetUIds(ImapFilter filter = null)
        {
            _mailFolder = await AuthenticateAsync();
            await _mailFolder.OpenAsync(FolderAccess.ReadOnly);

            if (filter == null)
            {
                _lastRetrievedUIds = await _mailFolder.SearchAsync(new SearchQuery());
            }
            else
            {
                _lastRetrievedUIds = await _mailFolder.SearchAsync(filter.ToSearchQuery());
            }

            if (_lastRetrievedUIds == null)
            {
                _lastRetrievedUIds = new List<UniqueId>();
            }

            return _lastRetrievedUIds;
        }

        public virtual async Task<List<MimeMessageUId>> GetMessageUids(IList<UniqueId> uniqueIds)
        {
            _mailFolder = await AuthenticateAsync();
            _mailFolder.Open(FolderAccess.ReadOnly);

            var list = new List<MimeMessageUId>();
            foreach (var uid in uniqueIds)
            {
                list.Add(new MimeMessageUId(await _mailFolder.GetMessageAsync(uid), uid));
            }
            return list;
        }

        public virtual async Task<MimeMessageUId> GetMessageUid(UniqueId uniqueId)
        {
            var mime = await _mailFolder.GetMessageAsync(uniqueId);
            if (mime != null)
            {
                return new MimeMessageUId(mime, uniqueId);
            }
            return null;
        }

        public virtual async Task<MimeMessageUId> GetMessage(UniqueId uniqueId)
        {
            _mailFolder = await AuthenticateAsync();
            await _mailFolder.OpenAsync(FolderAccess.ReadOnly);
            return new MimeMessageUId(await _mailFolder.GetMessageAsync(uniqueId), uniqueId);
        }

        public virtual async Task<IList<IMessageSummary>> GetSummaries()
        {
            var inbox = await AuthenticateAsync();
            await inbox.OpenAsync(FolderAccess.ReadOnly);
            return await inbox.FetchAsync(0, -1, MessageSummaryOptions);
        }

        public virtual async Task<IList<IMessageSummary>> GetSummaries(IList<UniqueId> uids)
        {
            var inbox = await AuthenticateAsync();
            await inbox.OpenAsync(FolderAccess.ReadOnly);
            return await inbox.FetchAsync(uids, MessageSummaryOptions);
        }

        public virtual async Task<IList<IMessageSummary>> GetSummaries(ImapFilter filter, uint[] uidsToExclude = null)
        {
            var inbox = await AuthenticateAsync();
            await inbox.OpenAsync(FolderAccess.ReadOnly);
            _lastRetrievedUIds = await GetUIds(filter);
            if (uidsToExclude?.Length > 0)
            {
                _lastRetrievedUIds = _lastRetrievedUIds.Where(x => !uidsToExclude.Contains(x.Id)).ToArray();
            }
            return await inbox.FetchAsync(_lastRetrievedUIds, MessageSummaryOptions);
        }

        public virtual async Task MarkAsRead(IList<UniqueId> uids)
        {
            if (_mailFolder == null)
            {
                _mailFolder = await AuthenticateAsync();
            }
            await _mailFolder.OpenAsync(FolderAccess.ReadWrite);
            await _mailFolder.SetFlagsAsync(uids, MessageFlags.Seen, false);
        }

        public virtual async Task DeleteMessages(IList<UniqueId> uids)
        {
            if (_mailFolder == null)
            {
                _mailFolder = await AuthenticateAsync();
            }
            await _mailFolder.OpenAsync(FolderAccess.ReadWrite);
            await _mailFolder.SetFlagsAsync(uids, MessageFlags.Deleted, false);
        }

        public virtual async Task DeleteMessages(ImapFilter imapFilter)
        {
            var uids = await GetUIds(imapFilter);
            await DeleteMessages(uids);
        }

        public virtual async Task ExpungeMail()
        {
            if (_mailFolder == null)
            {
                _mailFolder = await AuthenticateAsync();
            }
            await _mailFolder.OpenAsync(FolderAccess.ReadWrite);
            await _mailFolder.ExpungeAsync();
        }

        public virtual async Task<UniqueId?> AppendMessage(MimeMessage mimeMessage, bool asNotSeen = false)
        {
            var flags = asNotSeen ? MessageFlags.None : MessageFlags.Seen;
            if (_mailFolder == null)
            {
                _mailFolder = await AuthenticateAsync();
            }
            await _mailFolder.OpenAsync(FolderAccess.ReadWrite);
            return await _mailFolder.AppendAsync(mimeMessage);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected async virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (_mailFolder != null && _mailFolder.IsOpen)
                    {
                        await _mailFolder.CloseAsync();
                    }
                    if (_imapClient?.IsConnected == true)
                    {
                        await _imapClient.DisconnectAsync(true);
                        _imapClient.Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                _lastRetrievedUIds = null;
                _mailFolder = null;
                _mailFolders = null;

                disposedValue = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
        }
        #endregion
    }
}
