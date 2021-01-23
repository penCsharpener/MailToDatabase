using System;

namespace MailToDatabase.Sqlite.Configuration
{
    public class AppSettings
    {
        public string DownloadDirectory { get; set; } = "export";
        public string MailFolderName { get; set; }
        public int RetrievalIntervalMinutes { get; set; } = 10;
        public DateTime DelieveredAfter { get; set; }
        public DateTime DelieveredBefore { get; set; }
        public DateTime YoungerThan { get; set; }

        public Credentials Credentials { get; set; }
    }
}
