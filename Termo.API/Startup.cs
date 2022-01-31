using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Termo.API.Configurations;

namespace Termo.API
{
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {

            services.AddControllers()
                .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddSwaggerConfiguration();
            services.AddAuthenticationConfiguration();
            services.AddCorsConfiguration();
            services.AddEntityFrameworkConfiguration(Configuration);
            services.AddMemoryCache();
            services.AddRefitConfiguration();
            services.AddBackgroundTaskConfiguration();
            services.AddInjectionConfiguration();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if(env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwaggerConfiguration();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCorsConfiguration();

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
