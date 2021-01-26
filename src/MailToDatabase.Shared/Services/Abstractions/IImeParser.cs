using MailToDatabase.Shared.Models;
using System.Threading.Tasks;

namespace MailToDatabase.Shared.Services.Abstractions
{
    public interface IImeParser
    {
        Task ParseAsync(ImeFile file);
    }
}
