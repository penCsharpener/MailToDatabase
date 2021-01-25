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
using MailToDatabase.Contracts;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MailToDatabase
{
    public class MailTypeConverter : IDisposable
    {

        private IList<UniqueId> _lastRetrievedUids;
        public IList<uint> LastRetrievedUids => _lastRetrievedUids?.Select(x => x.Id).ToList();

        private readonly IRetrievalClient _client;

        public MailTypeConverter(IRetrievalClient client)
        {
            _client = client;
        }

        public async Task<List<ImapMessage>> GetMessages(ImapFilter filter = null,
                                                         uint[] UIdsToExclude = null)
        {
            var results = new List<ImapMessage>();
            await FilterAndExclude(filter, UIdsToExclude);
            var mimeMsgs = await _client.GetMessageUids(_lastRetrievedUids);

            foreach (var mime in mimeMsgs)
            {
                var imapMsg = await mime.ToImapMessage();
                imapMsg.MailFolder = _client.OpenedMailFolder;
                results.Add(imapMsg);
            }
            return results;
        }

        public async Task GetMessagesAsync(Func<ImapMessage, Task> func,
                                           ImapFilter filter = null,
                                           uint[] UIdsToExclude = null)
        {
            await FilterAndExclude(filter, UIdsToExclude);
            foreach (var uId in _lastRetrievedUids)
            {
                var mimeUid = await _client.GetMessageUid(uId);
                var imapMsg = await mimeUid.ToImapMessage();
                if (imapMsg != null)
                {
                    imapMsg.MailFolder = _client.OpenedMailFolder;
                    await func.Invoke(imapMsg);
                }
            }
        }

        private async Task FilterAndExclude(ImapFilter filter,
                                            uint[] UIdsToExclude)
        {
            _lastRetrievedUids = await _client.GetUIds(filter);
            if (UIdsToExclude != null && UIdsToExclude.Length > 0)
            {
                _lastRetrievedUids = _lastRetrievedUids.Where(x => !UIdsToExclude.Contains(x.Id)).ToList();
            }
            if (filter?.LimitResults > 0)
            {
                _lastRetrievedUids = _lastRetrievedUids.Take(filter.LimitResults).ToList();
            }
        }

        public async Task GetMessagesAsync(Func<ImapMessage, AsyncRetrievalInfo, Task> func,
                                           ImapFilter filter = null,
                                           uint[] UIdsToExclude = null)
        {

            await FilterAndExclude(filter, UIdsToExclude);

            var asyncInfo = new AsyncRetrievalInfo()
            {
                CountRetrievedMessages = _lastRetrievedUids.Count,
                UniqueIds = _lastRetrievedUids.Select(x => x.Id).ToArray(),
            };

            for (int i = 0; i < _lastRetrievedUids.Count; i++)
            {
                var mimeUid = await _client.GetMessageUid(_lastRetrievedUids[i]);
                var imapMsg = await mimeUid.ToImapMessage();
                if (imapMsg != null)
                {
                    imapMsg.MailFolder = _client.OpenedMailFolder;
                    asyncInfo.Index = i;
                    await func.Invoke(imapMsg, asyncInfo);
                }
            }
        }

        public async IAsyncEnumerable<ImapMessage> GetMessagesAsync(ImapFilter filter = null,
                                                                    uint[] UIdsToExclude = null)
        {
            await FilterAndExclude(filter, UIdsToExclude);
            for (int i = 0; i < _lastRetrievedUids.Count; i++)
            {
                var mimeUid = await _client.GetMessageUid(_lastRetrievedUids[i]);
                var imapMsg = await mimeUid.ToImapMessage();
                if (imapMsg != null)
                {
                    imapMsg.MailFolder = _client.OpenedMailFolder;
                    yield return imapMsg;
                }
            }
        }

        public static MailContact[] ConvertContacts(IEnumerable<MailboxAddress> mailAddresses)
        {
            var list = new List<MailContact>();
            foreach (var address in mailAddresses)
            {
                var newMailContact = new MailContact()
                {
                    ContactName = string.IsNullOrEmpty(address.Name) ? address.Address : address.Name,
                    EmailAddress = address.Address,
                };
                list.Add(newMailContact);
            }
            return list.ToArray();
        }

        public static async Task<ImapAttachment[]> ConvertAttachments(IEnumerable<MimeEntity> mimeEntities)
        {
            var list = new List<ImapAttachment>();
            foreach (var attachment in mimeEntities.OfType<MimePart>())
            {
                var newAttachment = new ImapAttachment()
                {
                    Filename = attachment.FileName,
                    FileContent = await attachment.ToBytes(),
                };
                list.Add(newAttachment);
            }
            return list.ToArray();
        }

        public static async Task<byte[]> SerializeMimeMessage(MimeMessage mimeMessage)
        {
            using (var ms = new MemoryStream())
            {
                await mimeMessage.WriteToAsync(ms);
                return ms.ToArray();
            }
        }

        public static async Task<ImapMessage> DeserializeMimeMessageAsync(byte[] mimeMessageBytes, uint? uId)
        {
            using (var ms = new MemoryStream(mimeMessageBytes))
            {
                var mime = await MimeMessage.LoadAsync(ms);
                return await mime.ToImapMessage(uId);
            }
        }

        public async Task MarkLastUidsAsRead(IMailToDatabaseClient dbClient)
        {
            if (_lastRetrievedUids != null && _lastRetrievedUids.Count > 0)
            {
                await dbClient.MarkAsRead(_lastRetrievedUids);
            }
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    _client.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.
                _lastRetrievedUids = null;

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
