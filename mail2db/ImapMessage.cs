using System;

namespace penCsharpener.Mail2DB {
    public class ImapMessage {

        public byte[] SerializedMessage { get; set; }
        public uint UId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public string BodyPlainText { get; set; }
        public bool HasAttachments { get; set; }
        public bool IsHTML { get; set; }
        public string MessageId { get; set; }
        public string InReplyToId { get; set; }
        public DateTime ReceivedAtUTC { get; set; }
        public DateTime ReceivedAtLocal { get; set; }
        public MailContact From { get; set; }
        public MailContact[] Cc { get; set; }
        public MailContact[] To { get; set; }
        public ImapAttachment[] Attachments { get; set; }

    }
}
