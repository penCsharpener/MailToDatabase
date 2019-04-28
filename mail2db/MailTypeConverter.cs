using MailKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace penCsharpener.Mail2DB {
    public class MailTypeConverter {

        public uint[] UIdsToExclude { get; set; }
        private IList<UniqueId> _lastRetrievedUids;
        public IList<uint> LastRetrievedUids => _lastRetrievedUids?.Select(x => x.Id).ToList();

        private readonly Client _client;

        public MailTypeConverter(Client client) {
            _client = client;
        }

        public async Task<List<ImapMessage>> GetMessages(ImapFilter filter = null) {
            var results = new List<ImapMessage>();
            _lastRetrievedUids = await _client.GetUIds(filter);
            if (UIdsToExclude != null) {
                _lastRetrievedUids = _lastRetrievedUids.Where(x => !UIdsToExclude.Contains(x.Id)).ToList();
            }
            var mimeMsgs = await _client.GetMessageUids(_lastRetrievedUids);

            foreach (var mime in mimeMsgs) {
                var imapMsg = await mime.ToImapMessage();
                imapMsg.MailFolder = _client.OpenedMailFolder;
                results.Add(imapMsg);
            }
            return results;
        }

        public async Task GetMessagesAsync(Func<ImapMessage, Task> func, ImapFilter filter = null) {
            _lastRetrievedUids = await _client.GetUIds(filter);
            if (UIdsToExclude != null) {
                _lastRetrievedUids = _lastRetrievedUids.Where(x => !UIdsToExclude.Contains(x.Id)).ToList();
            }
            foreach (var uId in _lastRetrievedUids) {
                var mimeUid = await _client.GetMessageUid(uId);
                var imapMsg = await mimeUid.ToImapMessage();
                if (imapMsg != null) {
                    imapMsg.MailFolder = _client.OpenedMailFolder;
                    await func.Invoke(imapMsg);
                }
            }

        }

        public static MailContact[] ConvertContacts(IEnumerable<MailboxAddress> mailAddresses) {
            var list = new List<MailContact>();
            foreach (var address in mailAddresses) {
                var newMailContact = new MailContact() {
                    ContactName = string.IsNullOrEmpty(address.Name) ? address.Address : address.Name,
                    EmailAddress = address.Address,
                };
                list.Add(newMailContact);
            }
            return list.ToArray();
        }

        public static async Task<ImapAttachment[]> ConvertAttachments(IEnumerable<MimeEntity> mimeEntities) {
            var list = new List<ImapAttachment>();
            foreach (var attachment in mimeEntities.OfType<MimePart>()) {
                var newAttachment = new ImapAttachment() {
                    Filename = attachment.FileName,
                    FileContent = await attachment.ToBytes(),
                };
                list.Add(newAttachment);
            }
            return list.ToArray();
        }

        public static async Task<byte[]> SerializeMimeMessage(MimeMessage mimeMessage) {
            using (var ms = new MemoryStream()) {
                await mimeMessage.WriteToAsync(ms);
                return ms.ToArray();
            }
        }

        public static async Task<ImapMessage> DeserializeMimeMessage(byte[] mimeMessageBytes, uint? uId) {
            using (var ms = new MemoryStream(mimeMessageBytes)) {
                var mime = await MimeMessage.LoadAsync(ms);
                return await mime.ToImapMessage(uId);
            }
        }

        public async Task MarkLastUidsAsRead() {
            if (_lastRetrievedUids != null && _lastRetrievedUids.Count > 0) {
                await _client.MarkAsRead(_lastRetrievedUids);
            }
        }
    }
}
