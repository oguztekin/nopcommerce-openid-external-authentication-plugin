using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.ExternalAuth.IdentityServer.Components
{
    [ViewComponent(Name = IdentityServerAuthenticationDefaults.ViewComponentName)]
    public class IdentityServerAuthenticationViewComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/ExternalAuth.IdentityServer/Views/PublicInfo.cshtml");
        }
    }
}