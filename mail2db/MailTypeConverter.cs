using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace penCsharpener.Mail2DB {
    public class MailTypeConverter {

        public uint[] UIdsToExclude { get; set; }

        private readonly Client _client;

        public MailTypeConverter(Client client) {
            _client = client;
        }

        public async Task<List<ImapMessage>> GetMessages() {
            var results = new List<ImapMessage>();
            var uIds = _client.GetUIds();
            if (UIdsToExclude != null) {
                uIds = uIds.Where(x => !UIdsToExclude.Contains(x.Id)).ToList();
            }
            var mimeMsgs = _client.GetMessages(uIds);

            foreach (var mime in mimeMsgs) {
                var imapMsg = await mime.ToImapMessage();
                results.Add(imapMsg);
            }
            return results;
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
                newAttachment.Filesize = (ulong)newAttachment.FileContent.Length;
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
    }
}
