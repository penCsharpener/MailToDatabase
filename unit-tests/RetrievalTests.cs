using NUnit.Framework;
using penCsharpener.Mail2DB;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace UnitTests {
    public class RetrievalTests {

        private Credentials Credentials { get; set; }
        private Client Mail2DB { get; set; }
        private MailTypeConverter Converter { get; set; }

        [SetUp]
        public async Task Setup() {
            if (!File.Exists(CredentialHelper.Filename)) {
                await CredentialHelper.WriteCredentials(new Credentials() {
                    EmailAddress = "someRandomMailxyz1234@gmail.com",
                    Password = "mySecret",
                    Port = 993,
                    ServerURL = "imap.gmail.com",
                });
            }
            Credentials = await CredentialHelper.GetCredentials();
            Mail2DB = new Client(Credentials);
            Converter = new MailTypeConverter(Mail2DB);
        }

        [Test]
        public void TestRetrieval() {
            var list = Mail2DB.GetUIds();

            Assert.IsTrue(list?.Count > 0);
        }

        [Test]
        public async Task TestConversion() {
            var list = await Converter.GetMessages();

            Assert.IsTrue(list?.Count > 0);
        }

        [Test]
        public async Task Deserialisation() {
            var list = await Converter.GetMessages();
            var oneMsg = list.FirstOrDefault();
            if (oneMsg != null) {
                var imapMsg = await MailTypeConverter.DeserializeMimeMessage(oneMsg.SerializedMessage, oneMsg.UId);
                Assert.True(oneMsg.Subject == imapMsg.Subject);
            } else Assert.Fail("No message was retrieved. Nothing to deserialise.");
        }

        [TearDown]
        public void CleanUp() {

        }
    }
}