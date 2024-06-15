using System.Collections.Concurrent;

namespace EcommerceApi.Services.BackgroundTaskService
{
    public class ProductTaskQueueService : IProductTaskQueueSerivce
    {
        private ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new();
        private SemaphoreSlim _signal = new(0);
        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            //it will wait until semephore release a task queue when QueuedHostedService call ExecuteAsync handle dequeue a task
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);
            return workItem;
        }

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            if(_workItems == null)
            {
                throw new ArgumentNullException(nameof(workItem));
            }
            _workItems.Enqueue(workItem);
            _signal.Release();
        }
    }
}
