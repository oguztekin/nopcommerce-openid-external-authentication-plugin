using Nop.Core.Configuration;

namespace Nop.Plugin.ExternalAuth.IdentityServer
{
    /// <summary>
    /// Represents settings of the Facebook authentication method
    /// </summary>
    public class IdentityServerExternalAuthSettings : ISettings
    {
        /// <summary>
        /// Gets or sets Ids client identifier
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Ids client secret
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Identity server url
        /// </summary>
        public string Authority { get; set; } = "http://localhost:5000";

        /// <summary>
        /// 
        /// </summary>
        public bool RequireHttpsMetadata { get; set; } = false;

        /// <summary>
        /// 
        /// </summary>
        public string ResponseType { get; set; } = "code id_token";

        /// <summary>
        /// 
        /// </summary>
        public bool SaveTokens { get; set; } = true;

        /// <summary>
        /// Scop list
        /// </summary>
        public string Scopes { get; set; } = "openid,profile,email";

        /// <summary>
        /// 
        /// </summary>
        public string NameClaimType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string RoleClaimType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string Audience { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public string ClaimsIssuer { get; set; }
    }
}
