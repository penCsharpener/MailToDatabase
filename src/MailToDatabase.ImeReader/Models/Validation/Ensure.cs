using System;

namespace MailToDatabase.ImeReader.Models.Validation
{
    public static class Ensure
    {
        public static void ThatNotNull(object value, string name)
        {
            if (value == null)
            {
                throw new ArgumentNullException(name);
            }
        }

        public static void ThatNotEmpty(string value, string name)
        {
            if (string.IsNullOrEmpty(value))
            {
                throw new ArgumentException(name);
            }
        }
    }
}
