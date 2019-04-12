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
                var imapMsg = new ImapMessage() {
                    SerializedMessage = await SerializeMimeMessage(mime.MimeMessage),
                    To = ConvertContacts(mime.MimeMessage.To.Mailboxes),
                    HasAttachments = mime.MimeMessage.Attachments.Any(x => x.IsAttachment),
                    Attachments = await ConvertAttachments(mime.MimeMessage.Attachments),
                    Body = mime.MimeMessage.HtmlBody,
                    BodyPlainText = mime.MimeMessage.TextBody,
                    Cc = ConvertContacts(mime.MimeMessage.Cc.Mailboxes),
                    From = ConvertContacts(mime.MimeMessage.From.Mailboxes)[0],
                    InReplyToId = mime.MimeMessage.InReplyTo,
                    MessageId = mime.MimeMessage.MessageId,
                    ReceivedAtLocal = mime.MimeMessage.Date.LocalDateTime,
                    ReceivedAtUTC = mime.MimeMessage.Date.UtcDateTime,
                    Subject = mime.MimeMessage.Subject,
                    UId = mime.UniqueId.Id,
                };

                results.Add(imapMsg);
            }
            return results;
        }

        private MailContact[] ConvertContacts(IEnumerable<MailboxAddress> mailAddresses) {
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

        private async Task<ImapAttachment[]> ConvertAttachments(IEnumerable<MimeEntity> mimeEntities) {
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

        private async Task<byte[]> SerializeMimeMessage(MimeMessage mimeMessage) {
            using (var ms = new MemoryStream()) {
                await mimeMessage.WriteToAsync(ms);
                return ms.ToArray();
            }
        }

    }
}
