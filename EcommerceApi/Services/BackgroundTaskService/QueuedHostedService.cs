using EcommerceApi.ExtensionExceptions;
using EcommerceApi.Extensions;
using System.Net;

namespace EcommerceApi.Services.BackgroundTaskService
{
    public class QueuedHostedService : BackgroundService
    {
        private readonly ILogger _logger;
        public IProductTaskQueueSerivce TaskQueue { get; }
        public QueuedHostedService(IProductTaskQueueSerivce taskQueue,
        ILoggerFactory loggerFactory) {
            TaskQueue = taskQueue;
            _logger = loggerFactory.CreateLogger<QueuedHostedService>();
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued Hosted Service is starting.");
            return ProcessTaskQueueAsync(stoppingToken);
        }
        private async Task ProcessTaskQueueAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var workItem = await TaskQueue.DequeueAsync(stoppingToken);

                try
                {
                    await workItem(stoppingToken);
                }
                catch(ProductStatusException pse)
                {
                    throw new ProductStatusException(pse.Status, pse.Message, pse.Result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                       $"Error occurred executing {nameof(workItem)}.");
                    throw new HttpStatusException(HttpStatusCode.InternalServerError, ex.Message);
                }
            }
        }
        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Queued Hosted Service is stopping.");
            await base.StopAsync(stoppingToken);
        }
    }
}
