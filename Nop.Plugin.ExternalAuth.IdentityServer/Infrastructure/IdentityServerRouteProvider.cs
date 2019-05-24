using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Localization;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.ExternalAuth.IdentityServer.Infrastructure
{
    public class IdentityServerRouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IRouteBuilder routeBuilder)
        {
            routeBuilder.MapLocalizedRoute("IdentityServerLogin", "login/", new { controller = "IdentityServerAuthentication", action = "Login" });
            routeBuilder.MapLocalizedRoute("IdentityServerRegister", "register/", new { controller = "IdentityServerAuthentication", action = "Login" });
            routeBuilder.MapLocalizedRoute("IdentityServerLogout", "logout/", new { controller = "IdentityServerAuthentication", action = "Logout" });
        }

        public int Priority => 1;
    }
}