namespace Nop.Plugin.ExternalAuth.IdentityServer
{
    /// <summary>
    /// Default values used by the Facebook authentication middleware
    /// </summary>
    public class IdentityServerAuthenticationDefaults
    {
        /// <summary>
        /// System name of the external authentication method
        /// </summary>
        public const string ProviderSystemName = "ExternalAuth.IdentityServer";

        /// <summary>
        /// Name of the view component to display plugin in public store
        /// </summary>
        public const string ViewComponentName = "IdentityServerAuthentication";
    }
}