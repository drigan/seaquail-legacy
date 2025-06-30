using SeaQuailDiagramTool.Domain.Services;

namespace SeaQuailDiagramTool.Application
{
    public class LocalUserProvider : IUserProvider
    {
        public (string email, string externalId)? GetCurrentUser()
        {
            // Return a default local user for development
            return ("local.user@dev.local", "local-user-id");
        }
    }
} 