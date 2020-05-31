using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityProvider.API.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OpenIddict.Core;
using OpenIddict.EntityFrameworkCore.Models;

namespace IdentityProvider.API
{
    public class Startup
    {
        public static readonly ILoggerFactory MyLoggerFactory
                            = LoggerFactory.Create(builder => { builder.AddConsole(); });

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddApplicationCorsService();
            services.AddApplicationSSlEnforce(false);
            services.AddApplicationIdentityService(Configuration.GetConnectionString("IdentityDbContext"));
            services.AddControllersWithViews();
            services.AddHostedService<IdentitySeedWorker>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseApplicationSSlEnforce(env, false);
            app.UseHttpsRedirection();
            app.UseRouting();

            app.UseApplicationCors();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapDefaultControllerRoute();
            });

            //FlushEntitiesAsync(app);
            app.UseWelcomePage();

        }

        private async void FlushEntitiesAsync(IApplicationBuilder app)
        {
            var lifetime = app.ApplicationServices.GetRequiredService<Microsoft.AspNetCore.Hosting.IApplicationLifetime>();

            while (!lifetime.ApplicationStopping.IsCancellationRequested)
            {
                // Create a new service scope to ensure the database context is correctly disposed when this methods returns.
                using (var scope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
                {
                    async Task DeleteAuthorizationsAsync()
                    {
                        var manager = scope.ServiceProvider.GetRequiredService<OpenIddictAuthorizationManager<OpenIddictEntityFrameworkCoreApplication<Guid>>>();
                        try
                        {
                            await manager.PruneAsync(lifetime.ApplicationStopping);
                        }

                        catch { }
                    }

                    async Task DeleteTokensAsync()
                    {
                        var manager = scope.ServiceProvider.GetRequiredService<OpenIddictTokenManager<OpenIddictEntityFrameworkCoreToken<Guid>>>();
                        try
                        {
                            await manager.PruneAsync(lifetime.ApplicationStopping);
                        }

                        catch { }
                    }

                    await DeleteAuthorizationsAsync();
                    await DeleteTokensAsync();
                    await Task.Delay(TimeSpan.FromMinutes(5));
                }
            }
        }
    }
}
