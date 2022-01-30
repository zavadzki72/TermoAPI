using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Text;
using Termo.API.Database;
using Termo.API.ExternalServices;
using Refit;
using Termo.API.BackgroundServices;

namespace Termo.API {
    public class Startup {
        public Startup(IConfiguration configuration) {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services) {

            services.AddControllers()
                .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            services.AddSwaggerGen(c => {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "TermoAPI", Version = "v1" });
            });

            var key = Encoding.ASCII.GetBytes("fedaf7d8863b48e197b9287d492b708e");
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x => {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddCors(options => {
                options.AddPolicy("AllowOnlyMyDomains", builder => {
                    builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .WithOrigins("https://termo-zavadzki72.vercel.app", "https://termo.vercel.app", "https://jogos.marccusz.com", "https://marccusz.com", "https://www.marccusz.com", "https://www.jogos.marccusz.com", "https://termo.marccusz.com", "https://www.termo.marccusz.com")
                    .AllowCredentials();
                });
            });

            services.AddCors(options => {
                options.AddPolicy("AllowAll", builder => {
                    builder
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .SetIsOriginAllowed(x => true)
                    .AllowCredentials();
                });
            });

            var connectionString = Configuration.GetConnectionString("DefaultConnection");

            services.AddDbContext<ApplicationDbContext>(options => {
                options.UseNpgsql(connectionString);
            }, ServiceLifetime.Scoped);

            services.AddMemoryCache();

            services
                .AddRefitClient<IDictionaryService>()
                .ConfigureHttpClient(x => {
                    x.BaseAddress = new Uri("https://significado.herokuapp.com");
                });

            services.AddHostedService<QueuedHostedService>();
            services.AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>();

            services.AddScoped<IWorldService, WorldService>();
            services.AddScoped<IStatisticsService, StatisticsService>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) {
            if(env.IsDevelopment()) {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "TermoAPI v1"));

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

#if DEBUG
            app.UseCors("AllowAll");
#else
            app.UseCors("AllowOnlyMyDomains");
#endif

            app.UseEndpoints(endpoints => {
                endpoints.MapControllers();
            });
        }
    }
}
