using System;

namespace penCsharpener.Mail2DB {
    public class ImapMessage {

        public byte[] MimeMessageBytes { get; set; }
        public uint UId { get; set; }
        public string MailFolder { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string BodyPlainText { get; set; }
        public bool HasAttachments { get; set; }
        public string MessageTextId { get; set; }
        public string InReplyToId { get; set; }
        public DateTime ReceivedAtUTC { get; set; }
        public DateTime ReceivedAtLocal { get; set; }
        public MailContact From { get; set; }
        public MailContact[] Cc { get; set; }
        public MailContact[] To { get; set; }
        public ImapAttachment[] Attachments { get; set; }

    }
}
