using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.ExternalAuth.IdentityServer.Models
{
    public class ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.ExternalAuth.IdentityServer.ClientKeyIdentifier")]
        public string ClientId { get; set; }

        [NopResourceDisplayName("Plugins.ExternalAuth.IdentityServer.ClientSecret")]
        public string ClientSecret { get; set; }

        [NopResourceDisplayName("Plugins.ExternalAuth.IdentityServer.Authority")]
        public string Authority { get; set; }

        [NopResourceDisplayName("Plugins.ExternalAuth.IdentityServer.RequireHttpsMetadata")]
        public bool RequireHttpsMetadata { get; set; }

        [NopResourceDisplayName("Plugins.ExternalAuth.IdentityServer.ResponseType")]
        public string ResponseType { get; set; }

        [NopResourceDisplayName("Plugins.ExternalAuth.IdentityServer.SaveTokens")]
        public bool SaveTokens { get; set; }

        [NopResourceDisplayName("Plugins.ExternalAuth.IdentityServer.Scopes")]
        public string Scopes { get; set; }

        [NopResourceDisplayName("Plugins.ExternalAuth.IdentityServer.NameClaimType")]
        public string NameClaimType { get; set; }

        [NopResourceDisplayName("Plugins.ExternalAuth.IdentityServer.RoleClaimType")]
        public string RoleClaimType { get; set; }

        [NopResourceDisplayName("Plugins.ExternalAuth.IdentityServer.Audience")]
        public string Audience { get; set; }

        [NopResourceDisplayName("Plugins.ExternalAuth.IdentityServer.ClaimsIssuer")]
        public string ClaimsIssuer { get; set; }
    }
}