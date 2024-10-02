using MassTransit;
using MassTransitTest.BackgroudServices.Base;
using MassTransitTest.Data;
using MassTransitTest.Events;
using Microsoft.EntityFrameworkCore;

namespace MassTransitTest.BackgroudServices
{
    public interface ISendFileBackgroudService : IScopedProcessingService
    {

    }

    public class SendFileBackgroudService : ISendFileBackgroudService
    {
        private readonly ILogger<SendFileBackgroudService> _logger;
        private readonly FileDatabaseContext _context;
        private readonly IPublishEndpoint _publishEndpoint;

        public SendFileBackgroudService(ILogger<SendFileBackgroudService> logger, FileDatabaseContext context, IPublishEndpoint publishEndpoint)
        {
            _logger = logger;
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        public async Task DoWorkAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
                    await SendFiles(stoppingToken);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in {backgroundService}", nameof(SendFileBackgroudService));
            }
        }

        private async Task SendFiles(CancellationToken stoppingToken)
        {
            try
            {
                //get files to send
                var filesToSend = _context.Files.Where(file => file.Status == FileStatus.Sending);
                var count = await filesToSend.CountAsync(stoppingToken);
                if (count <= 0)
                {
                    //no files found
                    _logger.LogInformation("No files to send");
                    return;
                }

                foreach (var file in filesToSend)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(500), stoppingToken);

                    var sequence = Random.Shared.NextInt64();

                    file.Status = FileStatus.FileSent;
                    await _publishEndpoint.Publish<FileSentEvent>(new FileSentEvent { Id = file.Id, Sequece = sequence, PrevSequence = sequence - 1 }, stoppingToken);
                    await _context.SaveChangesAsync(stoppingToken); //save files and publish event
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error chenking files to send");
            }
        }

        public Task StopAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
