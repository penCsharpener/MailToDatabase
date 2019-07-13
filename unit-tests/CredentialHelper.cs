using Newtonsoft.Json;
using penCsharpener.Mail2DB;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace UnitTests {
    public static class CredentialHelper {

        public const string Filename = ".mailCredentials";

        public static async Task<Credentials> GetCredentials() {
            if (!File.Exists(Filename)) {
                await WriteCredentials(new Credentials() {
                    EmailAddress = "someRandomMailxyz1234@gmail.com",
                    Password = "mySecret",
                    Port = 993,
                    ServerURL = "imap.gmail.com",
                });
                var finfo = new FileInfo(Filename);
                var proc = Process.Start($"cmd.exe /c vscode \"{finfo.FullName}\"");
            }
            var text = await File.ReadAllTextAsync(Filename);
            return JsonConvert.DeserializeObject<Credentials>(text);
        }

        public static async Task WriteCredentials(Credentials credentials) {
            var json = JsonConvert.SerializeObject(credentials, Formatting.Indented);
            await File.WriteAllTextAsync(Filename, json);
        }

    }
}
