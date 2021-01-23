using System;
using System.Collections.Generic;

namespace MailToDatabase.Sqlite.Domain.Models
{
    public class Email
    {
        public int Id { get; set; }
        public string FolderName { get; set; }
        public int UniqueId { get; set; }
        public string Sha256 { get; set; }
        public DateTime ReceivedAt { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int AttachmentCount { get; set; }
        public string FromAddress { get; set; }
        public string FileName { get; set; }

        public ICollection<Attachment> Attachments { get; set; }
    }
}
