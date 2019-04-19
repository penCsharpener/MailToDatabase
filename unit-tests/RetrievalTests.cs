using NUnit.Framework;
using penCsharpener.Mail2DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnitTests {
    public class RetrievalTests {

        private Credentials Credentials { get; set; }
        private Client Mail2DB { get; set; }
        private MailTypeConverter Converter { get; set; }
        private ImapFilter DefaultFilter { get; set; }

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
            DefaultFilter = new ImapFilter().NotSeen(); // younger than two days
        }

        [Test]
        public async Task TestRetrieval() {
            var list = await Mail2DB.GetUIds(DefaultFilter);

            Assert.IsTrue(list?.Count > 0);
        }

        [Test]
        public async Task FilterDeliveredBetween() {
            var filter = new ImapFilter().DeliveredBetween(new DateTime(2018, 5, 20), new DateTime(2018, 5, 25));
            var list = await Mail2DB.GetUIds(filter);

            Assert.IsTrue(list?.Count > 0);
        }

        [Test]
        public async Task FilterUids() {
            var filter = new ImapFilter().Uids(new List<uint>() { 15732, 17940, 4005, 6435, 2935, 11064});
            var list = await Mail2DB.GetUIds(filter);

            Assert.IsTrue(list?.Count > 0);
        }

        [Test]
        public async Task TestConversion() {
            var list = await Converter.GetMessages(DefaultFilter);

            Console.WriteLine("{0} messages converted.", list.Count);
            Assert.IsTrue(list?.Count > 0);
        }

        [Test]
        public async Task Deserialisation() {
            var list = await Converter.GetMessages(DefaultFilter);
            var oneMsg = list.FirstOrDefault();
            if (oneMsg != null) {
                var imapMsg = await MailTypeConverter.DeserializeMimeMessage(oneMsg.SerializedMessage, oneMsg.UId);
                Assert.True(oneMsg.Subject == imapMsg.Subject);
                Console.WriteLine("Deserialized message subject: '" + imapMsg.Subject + "'");
            } else Assert.Fail("No message was retrieved. Nothing to deserialise.");
        }

        [Test]
        public async Task SaveAttachment() {
            var existingFile = "saved/09/test_file.txt";
            if (File.Exists(existingFile)) {
                File.Delete(existingFile);
            }

            var txtContent = "pretend file contents";
            var txtBytes = Encoding.UTF8.GetBytes(txtContent);
            var attachment = new ImapAttachment() {
                FileContent = txtBytes,
                Filename = "test_file.txt",
            };

            var options = new FileSavingOption(FileSavingOptions.FirstTwoHashCharacters);

            await attachment.WriteFileAsync("saved", options: options);
            Assert.IsNotNull(attachment.FileInfo);
        }

        [Test]
        public async Task GetSpecificMailfolder() {
            Mail2DB.SetMailFolder("Sent");
            await Mail2DB.Authenticate();
            var count = (await Mail2DB.GetUIds()).Count;
            Assert.IsTrue(count > 0);
        }

        [TearDown]
        public void CleanUp() {

        }
    }
}