using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityProvider.API.Extensions
{
    public static class ApplicationBuilderExtension
    {
        /// <summary>
        /// Add the application cors middleware to allow cross origin call
        /// </summary>
        /// <param name="app"></param>
        /// <param name="corsPolicyName"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseApplicationCors(this IApplicationBuilder app, string corsPolicyName = "applicationcorspolicy")
        {
            app.UseCors(corsPolicyName);
            return app;
        }

        /// <summary>
        /// Enforces SSL and this is only allowed in production environment
        /// </summary>
        /// <param name="app"></param>
        /// <param name="env"></param>
        /// <param name="enforceSSl"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseApplicationSSlEnforce(this IApplicationBuilder app, IWebHostEnvironment env, bool enforceSSl = true)
        {
            if (enforceSSl && env.IsProduction())
            {
                app.UseHsts();
            }
            return app;
        }
    }
}
