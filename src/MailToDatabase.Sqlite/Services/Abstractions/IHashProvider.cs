namespace MailToDatabase.Sqlite.Services.Abstractions
{
    public interface IHashProvider
    {
        string ToSha1(byte[] bytes);
        string ToSha256(byte[] bytes);
    }
}
