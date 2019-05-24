using System;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Services.Authentication;
using Nop.Services.Authentication.External;

namespace Nop.Plugin.ExternalAuth.IdentityServer.Infrastructure
{
    /// <summary>
    /// Registration of Facebook authentication service (plugin)
    /// </summary>
    public class IdentityServerAuthenticationRegistrar : IExternalAuthenticationRegistrar
    {
        /// <summary>
        /// Configure
        /// </summary>
        /// <param name="builder">Authentication builder</param>
        public void Configure(AuthenticationBuilder builder)
        {
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            builder
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                var settings = EngineContext.Current.Resolve<IdentityServerExternalAuthSettings>();

                options.SignInScheme = NopAuthenticationDefaults.AuthenticationScheme;

                options.Authority = settings.Authority;
                options.RequireHttpsMetadata = settings.RequireHttpsMetadata;
                options.ClientId = settings.ClientId;
                options.ClientSecret = settings.ClientSecret;
                options.SaveTokens = settings.SaveTokens;
                options.ResponseType = settings.ResponseType;
                options.CallbackPath = new PathString("/plugins/IdentityServerAuthentication/LoginCallback");
                options.ClaimsIssuer = settings.ClaimsIssuer;

                if (!String.IsNullOrWhiteSpace(settings.NameClaimType))
                {
                    options.TokenValidationParameters.NameClaimType = settings.NameClaimType;
                }

                if (!String.IsNullOrWhiteSpace(settings.RoleClaimType))
                {
                    options.TokenValidationParameters.RoleClaimType = settings.RoleClaimType;
                }

                string[] scopes = settings.Scopes.Split(',');

                foreach (var scope in scopes)
                {
                    options.Scope.Add(scope.Trim());
                }

                options.Events = new OpenIdConnectEvents
                {
                    // handle the logout redirection
                    OnRedirectToIdentityProviderForSignOut = (context) =>
                    {
                        var logoutUri = $"{settings.Authority}/v2/logout?client_id={settings.ClientId}";

                        var postLogoutUri = context.Properties.RedirectUri;
                        if (!string.IsNullOrEmpty(postLogoutUri))
                        {
                            if (postLogoutUri.StartsWith("/"))
                            {
                                // transform to absolute
                                var request = context.Request;
                                postLogoutUri = request.Scheme + "://" + request.Host + request.PathBase + postLogoutUri;
                            }
                            logoutUri += $"&returnTo={ Uri.EscapeDataString(postLogoutUri)}";
                        }

                        context.Response.Redirect(logoutUri);
                        context.HandleResponse();

                        return Task.CompletedTask;
                    },
                    OnRedirectToIdentityProvider = context =>
                    {
                        context.ProtocolMessage.SetParameter("audience", settings.Audience);

                        return Task.FromResult(0);
                    },
                    OnRemoteFailure = context =>
                    {
                        context.Response.Redirect("/");
                        context.HandleResponse();

                        return Task.FromResult(0);
                    }
                };
            });
        }
    }
}
