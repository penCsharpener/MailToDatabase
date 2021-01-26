using MailToDatabase.Shared.Models;
using MailToDatabase.Shared.Services.Abstractions;
using MailToDatabase.Sqlite.Services.Abstractions;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace MailToDatabase.Shared.Services
{
    public class ImeParser : IImeParser
    {
        private readonly IFileSystem _fileSystem;
        private StringBuilder _stringBuilder = new();

        public ImeParser(IFileSystem fileSystem)
        {
            _fileSystem = fileSystem;
        }

        public async Task ParseAsync(ImeFile file)
        {
            var content = await _fileSystem.ReadAllTextAsync("Assets/emailPreview.html");

            if (string.IsNullOrEmpty(file.ImapMessage.Body))
            {
                _stringBuilder.Append(content).Replace("##Subject##", file.ImapMessage.Subject)
                        .Replace("##Body##", HtmlifyBody(file.ImapMessage.BodyPlainText));
            }
            else
            {
                _stringBuilder.Append(file.ImapMessage.Body);
            }

            var exportDir = Path.Combine(file.FileInfo.DirectoryName, file.FileInfo.Name.Replace(".ime", ""));

            if (!Directory.Exists(exportDir))
            {
                Directory.CreateDirectory(exportDir);
            }

            await File.WriteAllTextAsync(Path.Combine(exportDir, "export.html"), _stringBuilder.ToString());
        }

        private string HtmlifyBody(string bodyPlainText)
        {
            return bodyPlainText.Replace(Environment.NewLine, "<br>");
        }
    }
}
