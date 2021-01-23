namespace MailToDatabase.Sqlite.Extensions
{
    public static class StringExtensions
    {
        public static string Replace(this string text, char[] chars, char? replacement)
        {
            var result = text;

            foreach (var c in chars)
            {
                if (replacement == null)
                {
                    result = result.Replace($"{c}", string.Empty);
                }
                else
                {
                    result = result.Replace(c, replacement.Value);
                }
            }

            return result;
        }
    }
}
