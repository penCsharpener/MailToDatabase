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

        public static MailContact[] AddContactType(this IEnumerable<MailContact> contacts, ContactTypes type) {
            if (contacts == null || !contacts.Any()) {
                return new MailContact[] {};
            }
            var list = new List<MailContact>();
            foreach (var contact in contacts) {
                contact.ContactType = type;
                list.Add(contact);
            }
            return list.ToArray();
        }

        public static async Task<ImapMessage> ToImapMessage(this MimeMessage mime, uint? uId = null) {
             var newImapMsg = new ImapMessage() {
                MimeMessageBytes = await MailTypeConverter.SerializeMimeMessage(mime),
                To = MailTypeConverter.ConvertContacts(mime.To.Mailboxes).AddContactType(ContactTypes.To),
                HasAttachments = mime.Attachments.Any(x => x.IsAttachment),
                Attachments = await MailTypeConverter.ConvertAttachments(mime.Attachments),
                Body = mime.HtmlBody,
                BodyPlainText = mime.TextBody,
                Cc = MailTypeConverter.ConvertContacts(mime.Cc.Mailboxes).AddContactType(ContactTypes.Cc),
                From = MailTypeConverter.ConvertContacts(mime.From.Mailboxes).AddContactType(ContactTypes.From).FirstOrDefault(),
                InReplyToId = mime.InReplyTo,
                MessageTextId = mime.MessageId,
                ReceivedAtLocal = mime.Date.LocalDateTime,
                ReceivedAtUTC = mime.Date.UtcDateTime,
                Subject = mime.Subject,
            };

            newImapMsg.MessageTextId = mime.MessageId.IsNullOrEmpty() ? newImapMsg.MimeMessageBytes.ToSha256() : mime.MessageId;

            if (uId.HasValue) {
                newImapMsg.UId = uId.Value;
            }

            return newImapMsg;
        }
    }
}
