namespace MailToDatabase.Sqlite.Domain.Models
{
    public class Attachment
    {
        public int Id { get; set; }
        public int EmailId { get; set; }
        public string FileName { get; set; }
        public Email Email { get; set; }
    }
}
