using MassTransit;
using MassTransitTest.Data;
using Microsoft.EntityFrameworkCore;

namespace MassTransitTest.Events
{
    public class SendFileCommandConsummer : IConsumer<SendFileCommand>
    {
        private readonly ILogger<SendFileCommandConsummer> _logger;
        private readonly FileDatabaseContext _context;

        public SendFileCommandConsummer(ILogger<SendFileCommandConsummer> logger, FileDatabaseContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task Consume(ConsumeContext<SendFileCommand> context)
        {
            _logger.LogInformation("Send file command arrived {@fileData}", context.Message);

            //find file information
            var file = await _context.Files.FirstOrDefaultAsync(file => file.Id == context.Message.Id, context.CancellationToken);

            if (file is null)
            {
                _logger.LogWarning("File {id} not found", context.Message.Id);
                return;
            }

            _logger.LogInformation("File {id} found", file.Id);

            await Task.Delay(TimeSpan.FromSeconds(5));

            _logger.LogInformation("Send file command executed");

            //mark the file to be sent
            file.Status = FileStatus.Sending;

            await _context.SaveChangesAsync(context.CancellationToken);
        }
    }
}
