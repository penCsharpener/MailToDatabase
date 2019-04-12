using MailKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace penCsharpener.Mail2DB {
    public class MimeMessageUId {
        public MimeMessageUId(MimeMessage mimeMessage, UniqueId uniqueId) {
            MimeMessage = mimeMessage;
            UniqueId = uniqueId;
        }

        public MimeMessage MimeMessage { get; set; }
        public UniqueId UniqueId { get; set; }
    }
}
