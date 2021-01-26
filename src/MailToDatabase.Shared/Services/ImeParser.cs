using MailToDatabase.Shared.Models;
using MailToDatabase.Shared.Services.Abstractions;
using System.Text;
using System.Threading.Tasks;

namespace MailToDatabase.Shared.Services
{
    public class ImeParser : IImeParser
    {
        private StringBuilder _stringBuilder = new();

        public ImeParser()
        {
        }

        public async Task ParseAsync(ImeFile file)
        {

        }
    }
}
