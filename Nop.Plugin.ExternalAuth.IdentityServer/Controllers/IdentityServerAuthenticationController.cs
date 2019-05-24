using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Customers;
using Nop.Plugin.ExternalAuth.IdentityServer.Models;
using Nop.Services.Authentication.External;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Events;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using IAuthenticationService = Nop.Services.Authentication.IAuthenticationService;

namespace Nop.Plugin.ExternalAuth.IdentityServer.Controllers
{
    public class IdentityServerAuthenticationController : BasePluginController
    {
        #region Fields

        private readonly IdentityServerExternalAuthSettings _identityServerExternalAuthSettings;
        private readonly IExternalAuthenticationService _externalAuthenticationService;
        private readonly ILocalizationService _localizationService;
        private readonly IOptionsMonitorCache<FacebookOptions> _optionsCache;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IWorkContext _workContext;
        private readonly StoreInformationSettings _storeInformationSettings;
        private readonly IEventPublisher _eventPublisher;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IAuthenticationService _authenticationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotificationService _notificationService;

        #endregion

        #region Ctor

        public IdentityServerAuthenticationController(IdentityServerExternalAuthSettings identityServerExternalAuthSettings,
            IExternalAuthenticationService externalAuthenticationService,
            ILocalizationService localizationService,
            IOptionsMonitorCache<FacebookOptions> optionsCache,
            IPermissionService permissionService,
            ISettingService settingService,
            IEventPublisher eventPublisher,
            StoreInformationSettings storeInformationSettings,
            IWorkContext workContext,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            IAuthenticationService authenticationService,
            IHttpContextAccessor httpContextAccessor,
            INotificationService notificationService)
        {
            this._identityServerExternalAuthSettings = identityServerExternalAuthSettings;
            this._externalAuthenticationService = externalAuthenticationService;
            this._localizationService = localizationService;
            this._optionsCache = optionsCache;
            this._permissionService = permissionService;
            this._settingService = settingService;
            _eventPublisher = eventPublisher;
            _storeInformationSettings = storeInformationSettings;
            _workContext = workContext;
            _customerActivityService = customerActivityService;
            _genericAttributeService = genericAttributeService;
            _authenticationService = authenticationService;
            _httpContextAccessor = httpContextAccessor;
            _notificationService = notificationService;
        }

        #endregion

        #region Methods

        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure()
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return AccessDeniedView();

            var model = new ConfigurationModel
            {
                ClientId = _identityServerExternalAuthSettings.ClientId,
                ClientSecret = _identityServerExternalAuthSettings.ClientSecret,
                Authority = _identityServerExternalAuthSettings.Authority,
                RequireHttpsMetadata = _identityServerExternalAuthSettings.RequireHttpsMetadata,
                ResponseType = _identityServerExternalAuthSettings.ResponseType,
                SaveTokens = _identityServerExternalAuthSettings.SaveTokens,
                Scopes = _identityServerExternalAuthSettings.Scopes,
                NameClaimType = _identityServerExternalAuthSettings.NameClaimType,
                RoleClaimType = _identityServerExternalAuthSettings.RoleClaimType,
            };

            return View("~/Plugins/ExternalAuth.IdentityServer/Views/Configure.cshtml", model);
        }

        [HttpPost]
        [AdminAntiForgery]
        [AuthorizeAdmin]
        [Area(AreaNames.Admin)]
        public IActionResult Configure(ConfigurationModel model)
        {
            if (!_permissionService.Authorize(StandardPermissionProvider.ManageExternalAuthenticationMethods))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return Configure();

            //save settings
            _identityServerExternalAuthSettings.ClientId = model.ClientId;
            _identityServerExternalAuthSettings.ClientSecret = model.ClientSecret;
            _identityServerExternalAuthSettings.Authority = model.Authority;
            _identityServerExternalAuthSettings.RequireHttpsMetadata = model.RequireHttpsMetadata;
            _identityServerExternalAuthSettings.ResponseType = model.ResponseType;
            _identityServerExternalAuthSettings.SaveTokens = model.SaveTokens;
            _identityServerExternalAuthSettings.Scopes = model.Scopes;
            _identityServerExternalAuthSettings.NameClaimType = model.NameClaimType;
            _identityServerExternalAuthSettings.RoleClaimType = model.RoleClaimType;
            _settingService.SaveSetting(_identityServerExternalAuthSettings);

            //clear Facebook authentication options cache
            _optionsCache.TryRemove(OpenIdConnectDefaults.AuthenticationScheme);

            _notificationService.SuccessNotification(_localizationService.GetResource("Admin.Plugins.Saved"));

            return Configure();
        }

        public IActionResult Login(string returnUrl)
        {
            var authenticationProperties = new AuthenticationProperties
            {
                RedirectUri = Url.Action("LoginCallback", "IdentityServerAuthentication", new { returnUrl = returnUrl })
            };
            authenticationProperties.SetString("ErrorCallback", Url.RouteUrl("Login", new { returnUrl }));

            return Challenge(authenticationProperties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        public async Task<IActionResult> LoginCallback(string returnUrl)
        {
            //authenticate Facebook user
            var authenticateResult = await this.HttpContext.AuthenticateAsync(OpenIdConnectDefaults.AuthenticationScheme);
            if (!authenticateResult.Succeeded || !authenticateResult.Principal.Claims.Any())
                return RedirectToRoute("IdentityServerLogin");

            //create external authentication parameters
            var authenticationParameters = new ExternalAuthenticationParameters
            {
                ProviderSystemName = IdentityServerAuthenticationDefaults.ProviderSystemName,
                AccessToken = await this.HttpContext.GetTokenAsync(OpenIdConnectDefaults.AuthenticationScheme, "access_token"),
                Email = authenticateResult.Principal.FindFirst(claim => claim.Type == "email")?.Value,
                ExternalIdentifier = authenticateResult.Principal.FindFirst(claim => claim.Type == "sub")?.Value,
                ExternalDisplayIdentifier = authenticateResult.Principal.FindFirst(claim => claim.Type == "name")?.Value,
                Claims = authenticateResult.Principal.Claims.Select(claim => new ExternalAuthenticationClaim(claim.Type, claim.Value)).ToList()
            };

            //authenticate Nop user
            return _externalAuthenticationService.Authenticate(authenticationParameters, returnUrl);
        }

        [CheckAccessClosedStore(true)]
        [CheckAccessPublicStore(true)]
        public async Task Logout()
        {
            if (_workContext.OriginalCustomerIfImpersonated != null)
            {
                //activity log
                _customerActivityService.InsertActivity(_workContext.OriginalCustomerIfImpersonated, "Impersonation.Finished",
                    string.Format(_localizationService.GetResource("ActivityLog.Impersonation.Finished.StoreOwner"),
                        _workContext.CurrentCustomer.Email, _workContext.CurrentCustomer.Id),
                    _workContext.CurrentCustomer);

                _customerActivityService.InsertActivity("Impersonation.Finished",
                    string.Format(_localizationService.GetResource("ActivityLog.Impersonation.Finished.Customer"),
                        _workContext.OriginalCustomerIfImpersonated.Email, _workContext.OriginalCustomerIfImpersonated.Id),
                    _workContext.OriginalCustomerIfImpersonated);

                //logout impersonated customer
                _genericAttributeService
                    .SaveAttribute<int?>(_workContext.OriginalCustomerIfImpersonated, NopCustomerDefaults.ImpersonatedCustomerIdAttribute, null);

                return;
            }

            //activity log
            _customerActivityService.InsertActivity(_workContext.CurrentCustomer, "PublicStore.Logout",
                _localizationService.GetResource("ActivityLog.PublicStore.Logout"), _workContext.CurrentCustomer);

            //standard logout 
            await _httpContextAccessor.HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
            _authenticationService.SignOut();

            //raise logged out event       
            _eventPublisher.Publish(new CustomerLoggedOutEvent(_workContext.CurrentCustomer));

            //EU Cookie
            if (_storeInformationSettings.DisplayEuCookieLawWarning)
            {
                //the cookie law message should not pop up immediately after logout.
                //otherwise, the user will have to click it again...
                //and thus next visitor will not click it... so violation for that cookie law..
                //the only good solution in this case is to store a temporary variable
                //indicating that the EU cookie popup window should not be displayed on the next page open (after logout redirection to homepage)
                //but it'll be displayed for further page loads
                TempData["nop.IgnoreEuCookieLawWarning"] = true;
            }
        }

        #endregion
    }
}