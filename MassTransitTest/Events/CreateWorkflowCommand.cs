namespace MassTransitTest.Events
{
    public class CreateWorkflowCommand
    {
        public Guid Id { get; set; }
        public string? WorkflowId { get; set; } = default;
    }
}
