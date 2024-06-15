namespace EcommerceApi.Services.BackgroundTaskService
{
    public interface IProductTaskQueueSerivce
    {
        void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem);

        Task<Func<CancellationToken, Task>> DequeueAsync(
            CancellationToken cancellationToken);
    }
}
