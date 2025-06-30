using Azure.Data.Tables;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.WindowsAzure.Storage;
using SeaQuailDiagramTool.Domain.Models;
using SeaQuailDiagramTool.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeaQuailDiagramTool
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
            // Check if we should use local development mode
            var useLocalMode = Configuration.GetValue<bool>("UseLocalMode", true);
            var environment = Configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT", "Development");
            
            // Add logging to see what's happening
            var logger = services.BuildServiceProvider().GetService<ILogger<Startup>>();
            
            if (useLocalMode || environment == "Development")
            {
                // Use local services for development
                services
                    .AddHttpContextAccessor()
                    .AddSingleton<Infrastructure.LocalFilePersistenceService<User>>()
                    .AddSingleton<Infrastructure.LocalFilePersistenceService<Diagram>>()
                    .AddSingleton<Infrastructure.LocalFilePersistenceService<DiagramShare>>()
                    .AddSingleton(typeof(IPersistenceService<>), typeof(Infrastructure.LocalFilePersistenceService<>))
                    .AddSingleton<IUserProvider, Application.LocalUserProvider>()
                    .AddTransient<DiagramService>()
                    .AddTransient<CurrentUserService>()
                    .AddSingleton<Application.DiagramSerializer>();
            }
            else
            {
                // Use Azure services for production
                services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
                    .AddMicrosoftIdentityWebApp(Configuration.GetSection("AzureAd"));

                services.AddAuthorization(options =>
                {
                    // By default, all incoming requests will be authorized according to the default policy
                    options.FallbackPolicy = options.DefaultPolicy;
                });

                services
                    .AddHttpContextAccessor()
                    .AddSingleton(p => new TableServiceClient(Configuration.GetValue<string>("AzureStorage")))
                    .AddSingleton<Infrastructure.CloudTableKeyProvider>()
                    .AddSingleton<Application.DiagramSerializer>()
                    .AddSingleton(typeof(IPersistenceService<>), typeof(Infrastructure.CloudTablePersistenceService<>))
                    .AddSingleton<IUserProvider, Application.HttpUserProvider>()
                    .AddTransient<DiagramService>()
                    .AddTransient<CurrentUserService>();
            }

            services.AddRazorPages()
                .AddMvcOptions(options => { });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var useLocalMode = Configuration.GetValue<bool>("UseLocalMode", true);
            var environment = Configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT", "Development");

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            if (!useLocalMode && environment != "Development")
            {
                app.UseAuthentication();
                //app.UseAuthorization();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                
                if (useLocalMode || environment == "Development")
                {
                    endpoints.MapControllerRoute("Default", "{controller}/{action}", new { controller = "LocalAuth" });
                }
                else
                {
                    endpoints.MapControllerRoute("Default", "{controller}/{action}", new { controller = "Auth" });
                }
            });
        }
    }
}
