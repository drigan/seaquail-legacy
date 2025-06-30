using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SeaQuailDiagramTool.Domain.Models;
using SeaQuailDiagramTool.Domain.Services;

namespace SeaQuailDiagramTool
{
    public class StartupDevelopment
    {
        public static void ConfigureDevelopmentServices(IServiceCollection services, IConfiguration configuration)
        {
            // Use local services instead of Azure services
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

        public static void ConfigureDevelopment(IApplicationBuilder app)
        {
            // No authentication middleware needed for local development
            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
                endpoints.MapControllers();
                endpoints.MapControllerRoute("Default", "{controller}/{action}", new { controller = "LocalAuth" });
            });
        }
    }
} 