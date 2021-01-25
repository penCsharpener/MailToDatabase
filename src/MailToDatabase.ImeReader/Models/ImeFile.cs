using System.IO;

namespace MailToDatabase.ImeReader.Models
{
    public class ImeFile
    {
        public FileInfo FileInfo { get; set; }
        public ImapMessage ImapMessage { get; set; }
    }
}
