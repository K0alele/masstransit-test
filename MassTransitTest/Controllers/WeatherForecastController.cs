using MassTransit;
using MassTransitTest.Data;
using MassTransitTest.Events;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MassTransitTest.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        private readonly ILogger<WeatherForecastController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly FileDatabaseContext _dbContext;

        public WeatherForecastController(ILogger<WeatherForecastController> logger, IPublishEndpoint publishEndpoint, FileDatabaseContext dbContext)
        {
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _dbContext = dbContext;
        }

        [HttpPost("File/Create")]
        public async Task<IActionResult> File([FromForm] string data, [FromForm] string WorkflowId, CancellationToken cancellationToken)
        {
            try
            {
                var id = NewId.NextGuid();

                await _dbContext.Files.AddAsync(new FileData { Id = id, Data = data, Created = DateTimeOffset.Now, Status = FileStatus.Processing }, cancellationToken);
                await _publishEndpoint.Publish<ProcessFileCommand>(new ProcessFileCommand { Id = id, WorkflowId = WorkflowId }, cancellationToken);

                await _dbContext.SaveChangesAsync(cancellationToken);

                return CreatedAtAction("GetFileInfo", new { id }, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending command");
                return StatusCode(500, ex.Message);
            }
        }

        [HttpGet("File/{id}")]
        public async Task<IActionResult> GetFileInfo(Guid id, CancellationToken cancellationToken)
        {
            try
            {
                var file = await _dbContext.Files.FirstOrDefaultAsync(file => file.Id == id, cancellationToken);

                if (file is null)
                    return NotFound();

                return Ok(file);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error");
                return StatusCode(500, ex.Message);
            }
        }
    }
}
