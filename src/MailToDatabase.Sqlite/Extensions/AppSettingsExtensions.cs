using MailToDatabase.Sqlite.Configuration;

namespace MailToDatabase.Sqlite.Extensions
{
    public static class AppSettingsExtensions
    {
        public static ImapFilter ToIntervalFilter(this AppSettings settings)
        {
            var filter = new ImapFilter().SentBetween(settings.DelieveredAfter, settings.DelieveredBefore);

            return filter;
        }
    }
}
