using Microsoft.Extensions.DependencyInjection;
using Termo.API.BackgroundServices;

namespace Termo.API.Configurations
{
    public static class BackgroundTaskConfiguration
    {

        public static void AddBackgroundTaskConfiguration(this IServiceCollection services)
        {
            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();
        }

    }
}
