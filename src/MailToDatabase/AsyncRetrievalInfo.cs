namespace MailToDatabase
{
    public class AsyncRetrievalInfo
    {
        public int CountRetrievedMessages { get; internal set; }
        public uint[] UniqueIds { get; internal set; }
        public int Index { get; internal set; }
    }
}
