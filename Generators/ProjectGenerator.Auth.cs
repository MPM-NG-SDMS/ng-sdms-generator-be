using System;
using System.IO;
using System.Text;

namespace NgSdms.Master.Generator
{
    public partial class ProjectGenerator
    {
        // Implementation of CreateKeycloakAuthExtensions moved here
        partial void CreateKeycloakAuthExtensions(string projectDir)
        {
            var authDir = Path.Combine(projectDir, "Auth");
            Directory.CreateDirectory(authDir);

            // Create KeycloakAuthExtensions.cs
            var keycloakExtensionsContent = new StringBuilder();
            // ... all original code building and writing KeycloakAuthExtensions.cs ...

            File.WriteAllText(Path.Combine(authDir, "KeycloakAuthExtensions.cs"), keycloakExtensionsContent.ToString());

            // Create KeycloakTokenValidationMiddleware.cs
            var tokenMiddlewareContent = new StringBuilder();
            // ... original code for KeycloakTokenValidationMiddleware ...

            File.WriteAllText(Path.Combine(authDir, "KeycloakTokenValidationMiddleware.cs"), tokenMiddlewareContent.ToString());

            // Create TokenClaimsLoggingMiddleware.cs
            var claimsMiddlewareContent = new StringBuilder();
            // ... original code for TokenClaimsLoggingMiddleware ...

            File.WriteAllText(Path.Combine(authDir, "TokenClaimsLoggingMiddleware.cs"), claimsMiddlewareContent.ToString());

            // Create HttpContextExtensions.cs
            var httpContextExtContent = new StringBuilder();
            // ... original code for HttpContextExtensions ...

            File.WriteAllText(Path.Combine(authDir, "HttpContextExtensions.cs"), httpContextExtContent.ToString());
        }
    }
}
