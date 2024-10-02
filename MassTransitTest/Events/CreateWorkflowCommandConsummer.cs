using MassTransit;
using MassTransitTest.Data;
using Microsoft.EntityFrameworkCore;

namespace MassTransitTest.Events
{
    public class CreateWorkflowCommandConsummer : IConsumer<CreateWorkflowCommand>
    {
        private readonly ILogger<CreateWorkflowCommandConsummer> _logger;
        private readonly FileDatabaseContext _context;

        public CreateWorkflowCommandConsummer(ILogger<CreateWorkflowCommandConsummer> logger, FileDatabaseContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task Consume(ConsumeContext<CreateWorkflowCommand> context)
        {
            if (string.IsNullOrWhiteSpace(context.Message?.WorkflowId))
            {
                _logger.LogWarning("No workflow assigned to {id}", context.Message!.Id);
                await context.Publish<WorkflowCreatedEvent>(new WorkflowCreatedEvent { Id = context.Message.Id, WorkflowId = null }, context.CancellationToken);
                await _context.SaveChangesAsync(context.CancellationToken);
                return;
            }

            //find file information
            var file = await _context.Files.FirstOrDefaultAsync(file => file.Id == context.Message.Id, context.CancellationToken);

            if (file is null)
            {
                _logger.LogWarning("File {id} not found", context.Message.Id);
                await context.Publish<WorkflowCreatedEvent>(new WorkflowCreatedEvent { Id = context.Message.Id, WorkflowId = null }, context.CancellationToken);
                await _context.SaveChangesAsync(context.CancellationToken);
                return;
            }

            _logger.LogInformation("Starting Workflow creation for {id}", context.Message.Id);

            await Task.Delay(TimeSpan.FromSeconds(5));

            var retries = context.GetRetryCount();

            if (retries <= 0)
                throw new Exception("Throw an exception to simulate a retry");

            file.Status = FileStatus.Finished;

            await context.Publish<WorkflowCreatedEvent>(new WorkflowCreatedEvent { Id = context.Message.Id, WorkflowId = Guid.NewGuid().ToString() }, context.CancellationToken);
            await _context.SaveChangesAsync(context.CancellationToken);
        }
    }
}
