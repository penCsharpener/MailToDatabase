using System.Threading.Tasks;

namespace penCsharpener.Mail2DB.Contracts {
    public interface IWriteAttachment {
        Task WriteFileAsync(string path, FileSavingOption options = null, string filename = null, bool overwrite = false);
    }
}