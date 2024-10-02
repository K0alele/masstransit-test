namespace MassTransitTest.Events
{
    public class FileSentEvent
    {
        public Guid Id { get; set; }
        public long Sequece { get; set; }
        public long PrevSequence { get; set; }
    }
}
