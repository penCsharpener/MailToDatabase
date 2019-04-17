using MimeKit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace penCsharpener.Mail2DB {
    public static class MailkitExtensions {
        public static async Task<byte[]> ToBytes(this MimePart mimePart) {
            using (var ms = new MemoryStream()) {
                await mimePart.Content.DecodeToAsync(ms);
                return ms.ToArray();
            }
        }

        public static async Task<ImapMessage> ToImapMessage(this MimeMessageUId mime) {
            return await mime.MimeMessage.ToImapMessage(mime.UniqueId.Id);
        }

        public static async Task<ImapMessage> ToImapMessage(this MimeMessage mime, uint? uId = null) {
             var newImapMsg = new ImapMessage() {
                SerializedMessage = await MailTypeConverter.SerializeMimeMessage(mime),
                To = MailTypeConverter.ConvertContacts(mime.To.Mailboxes),
                HasAttachments = mime.Attachments.Any(x => x.IsAttachment),
                Attachments = await MailTypeConverter.ConvertAttachments(mime.Attachments),
                Body = mime.HtmlBody,
                BodyPlainText = mime.TextBody,
                Cc = MailTypeConverter.ConvertContacts(mime.Cc.Mailboxes),
                From = MailTypeConverter.ConvertContacts(mime.From.Mailboxes)[0],
                InReplyToId = mime.InReplyTo,
                MessageId = mime.MessageId,
                ReceivedAtLocal = mime.Date.LocalDateTime,
                ReceivedAtUTC = mime.Date.UtcDateTime,
                Subject = mime.Subject,
            };

            if (uId.HasValue) {
                newImapMsg.UId = uId.Value;
            }

            return newImapMsg;
        }
    }
}
