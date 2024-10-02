namespace MassTransitTest.Data
{
    public class FileData
    {
        public Guid Id { get; set; }
        public string Data { get; set; } = default!;
        public DateTimeOffset Created { get; set; }
        public FileStatus Status { get; set; }
    }

    public enum FileStatus
    {
        Processing,
        Sending,
        FileSent,
        Finished
    }
}
