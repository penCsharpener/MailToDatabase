using FluentAssertions;
using MailToDatabase.Clients;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnitTests;
using Xunit;

namespace MailToDatabase.Tests.Integration
{
    public class RetrievalTests
    {

        private Credentials Credentials { get; }
        private RetrievalConfiguration RetrievalConfiguration { get; }
        private GenericClient Mail2DB { get; }
        private MailTypeConverter Converter { get; }
        private ImapFilter DefaultFilter { get; }

        public RetrievalTests()
        {
            Credentials = ConfigHelper.GetCredentials();
            RetrievalConfiguration = ConfigHelper.GetRetrievalConfiguration();
            Mail2DB = new GenericClient(Credentials);
            Converter = new MailTypeConverter(Mail2DB);
            DefaultFilter = new ImapFilter().NotSeen(); // younger than two days
        }

        [Fact]
        public async Task GetSpecificMailFolder()
        {
            Mail2DB.SetMailFolder("inbox");

            var list = await Mail2DB.GetUIds(DefaultFilter);

            list?.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task GetMailFolders()
        {
            var list = await Mail2DB.GetMailFoldersAsync();

            list?.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task TestRetrieval()
        {
            var list = await Mail2DB.GetUIds(DefaultFilter);

            list?.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task FilterDeliveredBetween()
        {
            var filter = new ImapFilter().DeliveredBetween(RetrievalConfiguration.RetrieveFrom, RetrievalConfiguration.RetrieveTo);
            var list = await Mail2DB.GetUIds(filter);

            list?.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task FilterUids()
        {
            var filter = new ImapFilter().Uids(RetrievalConfiguration.UidsToRetrieve);
            var list = await Mail2DB.GetUIds(filter);

            list?.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task TestConversion()
        {
            var list = await Converter.GetMessages(new ImapFilter().Uids(RetrievalConfiguration.UidWithAttachments));

            Console.WriteLine("{0} messages converted.", list.Count);
            list?.Count.Should().BeGreaterThan(0);
        }

        [Fact]
        public async Task Deserialisation()
        {
            var list = await Converter.GetMessages(new ImapFilter().Uids(RetrievalConfiguration.UidWithAttachments));
            var oneMsg = list.FirstOrDefault();

            oneMsg.Should().NotBeNull();
            var imapMsg = await MailTypeConverter.DeserializeMimeMessage(oneMsg.MimeMessageBytes, oneMsg.UId);
            oneMsg.Subject.Should().Be(imapMsg.Subject);
            Console.WriteLine("Deserialized message subject: '" + imapMsg.Subject + "'");
        }

        [Fact]
        public async Task SaveAttachment()
        {
            var existingFile = "saved/09/test_file.txt";
            if (File.Exists(existingFile))
            {
                File.Delete(existingFile);
            }

            var txtContent = "pretend file contents";
            var txtBytes = Encoding.UTF8.GetBytes(txtContent);
            var attachment = new ImapAttachment()
            {
                FileContent = txtBytes,
                Filename = "test_file.txt",
            };

            var options = new FileSavingOption(FileSavingOptions.FirstTwoHashCharacters);

            await attachment.WriteFileAsync("saved", options: options);
            attachment.FileInfo.Should().NotBeNull();
        }

        [Fact]
        public async Task GetSpecificMailfolder()
        {
            Mail2DB.SetMailFolder("Sent");
            await Mail2DB.AuthenticateAsync();
            var count = (await Mail2DB.GetUIds()).Count;
            count.Should().BeGreaterThan(0);
        }
    }
}