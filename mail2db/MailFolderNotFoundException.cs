using System;
using System.Collections.Generic;
using System.Text;

namespace penCsharpener.Mail2DB {
    public class MailFolderNotFoundException : Exception {
        public MailFolderNotFoundException() : base() {
        }

        public MailFolderNotFoundException(string message) : base(message) {
        }

        public MailFolderNotFoundException(string message, Exception innerException) : base(message, innerException) {
        }
    }
}
