using Newtonsoft.Json;
using penCsharpener.Mail2DB;
using System.IO;
using System.Threading.Tasks;

namespace UnitTests {
    public static class CredentialHelper {

        public const string Filename = ".mailCredentials";

        public static async Task<Credentials> GetCredentials() {
            if (File.Exists(Filename)) {
                var text = await File.ReadAllTextAsync(Filename);
                return JsonConvert.DeserializeObject<Credentials>(text);
            }
            return new Credentials();
        }

        public static async Task WriteCredentials(Credentials credentials) {
            var json = JsonConvert.SerializeObject(credentials, Formatting.Indented);
            await File.WriteAllTextAsync(Filename, json);
        }

    }
}
