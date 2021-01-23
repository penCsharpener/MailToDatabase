using System.Threading.Tasks;

namespace MailToDatabase.Sqlite.Services.Abstractions
{
    public interface IImapService
    {
        Task GetAllMailsAsync();
    }
}
