using System;

namespace MailToDatabase.Tests.Integration
{
    public class RetrievalConfiguration
    {
        public uint[] UidsToRetrieve { get; set; }
        public uint UidWithAttachments { get; set; }
        public DateTime RetrieveFrom { get; set; }
        public DateTime RetrieveTo { get; set; }
    }
}
