namespace MassTransitTest.Events
{
    public class ProcessFileCommand
    {
        public Guid Id { get; set; }
        public string? WorkflowId { get; set; } = default;
    }
}
