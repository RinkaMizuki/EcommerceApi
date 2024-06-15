using EcommerceApi.Models;
using Microsoft.EntityFrameworkCore;

namespace EcommerceApi.Services.BackgroundTaskService
{
    public class ExpiredUserCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        public ExpiredUserCleanupService(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    using (var scope = _serviceScopeFactory.CreateScope())
            //    {
            //        var dbContext = scope.ServiceProvider.GetRequiredService<EcommerceDbContext>();
            //        var currentDate = DateTime.UtcNow;

            //        var expiredUsers = await dbContext.RefreshTokens
            //            .Where(rf => rf.Expires <= currentDate)
            //            .ToListAsync(stoppingToken);

            //        foreach (var user in expiredUsers)
            //        {
            //            dbContext.RefreshTokens.RemoveRange(expiredUsers);
            //            await dbContext.SaveChangesAsync(stoppingToken);
            //        }
            //    }
            //    Console.WriteLine($"Re call");
            //    // Kiểm tra và xóa các bản ghi đã hết hạn mỗi 1 giờ
            //    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            //    await base.StopAsync(stoppingToken);
            //}
        }
    }
}
