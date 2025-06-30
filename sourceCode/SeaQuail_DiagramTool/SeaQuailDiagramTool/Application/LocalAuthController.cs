using Microsoft.AspNetCore.Mvc;
using SeaQuailDiagramTool.Domain.Services;
using System.Threading.Tasks;

namespace SeaQuailDiagramTool.Application
{
    public class LocalAuthController : Controller
    {
        private readonly CurrentUserService currentUserService;

        public LocalAuthController(CurrentUserService currentUserService)
        {
            this.currentUserService = currentUserService;
        }

        [HttpGet]
        public async Task<IActionResult> SignIn()
        {
            // For local development, automatically create the user and redirect
            await currentUserService.CreateIfNotExists();
            return Redirect("/close");
        }

        [HttpGet]
        public IActionResult SignOutLocal()
        {
            // For local development, just redirect to home
            return Redirect("/");
        }

        [HttpGet]
        public async Task<IActionResult> EstablishUser()
        {
            await currentUserService.CreateIfNotExists();
            return Redirect("/close");
        }
    }
} 