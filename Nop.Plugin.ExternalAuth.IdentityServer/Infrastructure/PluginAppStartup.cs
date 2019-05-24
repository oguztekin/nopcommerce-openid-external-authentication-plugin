using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;

namespace Nop.Plugin.ExternalAuth.IdentityServer.Infrastructure
{
    public class PluginAppStartup : INopStartup
    {
        public int Order => 501;

        public void Configure(IApplicationBuilder application)
        {
            application.UseAuthentication();
        }

        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {

        }
    }
}
