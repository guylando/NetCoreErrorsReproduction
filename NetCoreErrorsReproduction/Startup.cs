using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetCoreErrorsReproduction.Identity;
using NetCoreErrorsReproduction.Middleware;

namespace NetCoreErrorsReproduction
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IIdentityUser, IdentityUser>();
            services.AddTransient<IIdentityRole, IdentityRole>();
            services.AddIdentity<IIdentityUser, IIdentityRole>();
            services.AddSignalR(options => {
                // Uncomment for debugging purposes
                // options.Hubs.EnableDetailedErrors = true;
            });
            services.AddAntiforgery(options => {
                options.Cookie.Path = "/";
            });
            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, 
            ILoggerFactory loggerFactory, IHttpContextAccessor contextAccessor, IServiceProvider serviceProvider)
        {
            loggerFactory.AddPostgresql(serviceProvider, contextAccessor);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseAuthentication();

            app.UseMiddleware<HideExceptionsMiddleware>();

            app.UseStaticFiles();

            // Add support for web sockets
            app.UseWebSockets();
            
            app.UseSignalR("/ff");

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
