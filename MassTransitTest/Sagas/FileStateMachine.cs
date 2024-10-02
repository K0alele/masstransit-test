using MassTransit;
using MassTransitTest.Events;

namespace MassTransitTest.Sagas
{
    public class FileState : SagaStateMachineInstance
    {
        public Guid CorrelationId { get; set; }
        public int CurrentState { get; set; }

        public string? WorkflowId { get; set; }
        public long Sequence { get; set; }
        public long PrevSequece { get; set; }

        public bool FileSent { get; set; } = false;
        public bool WorkflowCreated { get; set; } = false;
    }

    public class FileStateMachine : MassTransitStateMachine<FileState>
    {
        public FileStateMachine()
        {
            InstanceState(x => x.CurrentState, Sending, CreatingWorkflow, Finished);
            Event(() => ProcessFile, x => x.CorrelateById(context => context.Message.Id));
            Event(() => FileSent, x => x.CorrelateById(context => context.Message.Id));
            Event(() => WorkflowCreated, x => x.CorrelateById(context => context.Message.Id));

            Initially(
                When(ProcessFile)
                    .Then(context =>
                    {
                        context.Saga.WorkflowId = context.Message.WorkflowId;
                    })
                    .TransitionTo(Sending)
                    .Publish(context => new SendFileCommand { Id = context.Saga.CorrelationId })
            );

            During(Sending,
                When(FileSent)
                .Then(context =>
                {
                    context.Saga.Sequence = context.Message.Sequece;
                    context.Saga.PrevSequece = context.Message.PrevSequence;
                    context.Saga.FileSent = true;
                })
                .TransitionTo(CreatingWorkflow)
                .Publish(context => new CreateWorkflowCommand { Id = context.Saga.CorrelationId, WorkflowId = context.Saga.WorkflowId })
            );

            During(CreatingWorkflow,
                When(WorkflowCreated)
                .Then(context =>
                {
                    context.Saga.WorkflowId = context.Message.WorkflowId;
                    context.Saga.WorkflowCreated = true;
                })
                .TransitionTo(Finished)
                .Finalize()
            );

            SetCompletedWhenFinalized();
        } 

        public Event<ProcessFileCommand> ProcessFile { get; private set; }
        public Event<FileSentEvent> FileSent { get; private set; }
        public Event<WorkflowCreatedEvent> WorkflowCreated { get; private set; }

        public State Sending { get; private set; }
        public State CreatingWorkflow { get; private set; }
        public State Finished { get; private set; }
    }
}
