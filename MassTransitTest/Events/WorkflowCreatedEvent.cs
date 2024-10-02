namespace MassTransitTest.Events
{
    public class WorkflowCreatedEvent
    {
        public Guid Id { get; set; }
        public string? WorkflowId { get; set; }
    }
}
