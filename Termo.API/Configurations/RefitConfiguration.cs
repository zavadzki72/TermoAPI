using Microsoft.Extensions.DependencyInjection;
using Refit;
using System;
using Termo.Models.ExternalServices;

namespace Termo.API.Configurations
{
    public static class RefitConfiguration
    {

        public static void AddRefitConfiguration(this IServiceCollection services)
        {
            services.AddRefitClient<IDictionaryService>()
                .ConfigureHttpClient(x => {
                    x.BaseAddress = new Uri("https://significado.herokuapp.com");
                });
        }

    }
}
