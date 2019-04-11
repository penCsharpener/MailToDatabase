namespace penCsharpener.Mail2DB {
    public class ImapAttachment {
        public string Filename { get; set; }
        public ulong Filesize { get; set; }
        public byte[] FileContent { get; set; }
    }
}
