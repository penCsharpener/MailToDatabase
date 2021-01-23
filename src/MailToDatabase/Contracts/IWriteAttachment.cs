using System.Threading.Tasks;

namespace MailToDatabase.Contracts
{
    public interface IWriteAttachment
    {
        Task WriteFileAsync(string path, FileSavingOption options = null, string filename = null, bool overwrite = false);
    }
}