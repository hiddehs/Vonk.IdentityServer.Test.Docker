using System.Linq;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vonk.IdentityServer.SOF.KeyManagement;
using Vonk.IdentityServer.SOF.SmartTokenValidation;
using Vonk.IdentityServer.Test.ProfileService;
using Vonk.IdentityServer.Test.SmartTokenValidation;
using Vonk.IdentityServer.Test.Support;

namespace Vonk.IdentityServer
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IConfiguration configuration)
        {
            Check.NotNull(configuration, nameof(configuration));
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddOptions();
            services.Configure<FHIRServerConfig>(_configuration.GetSection("FHIRServerConfig"));
            services.Configure<KeyManagementConfig>(_configuration.GetSection("KeyManagementConfig"));

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = (SameSiteMode)(-1);
                options.OnAppendCookie = cookieContext =>
                CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
                options.OnDeleteCookie = cookieContext =>
                    CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            });

            var identityServerBuilder = services.AddIdentityServer(Config.GetOptions);

            // Add identities and clients
            identityServerBuilder
                .AddTestUsers(Config.GetUsers())
                .AddInMemoryIdentityResources(Config.GetIdentityResources())
                .AddInMemoryClients(Config.GetClients());

            // Add scopes and resources
            identityServerBuilder
                .AddInMemoryApiScopes(Config.GetApiScopes())
                .AddInMemoryApiResources(Config.GetApiResources());

            // Add custom SMART on FHIR validation services
            identityServerBuilder
                .AddCustomTokenRequestValidator<SmartAccessTokenValidator>()
                .AddCustomAuthorizeRequestValidator<AudienceAuthorizeRequestValidator>()
                .AddProfileService<SmartPatientContextProfileService>();

            // Add signing credentials
            identityServerBuilder
                .AddJwtBearerClientAuthentication();

            // Enable AddDeveloperSigningCredential instead of the ISigningCredentialStore and IValidationKeysStore below if you don't want to configure the signing and valdidation keys
            // identityServerBuilder.AddDeveloperSigningCredential();

            services.AddSingleton<ISigningCredentialStore, SmartKeyStore>();
            services.AddSingleton<IValidationKeysStore, SmartKeyStore>();

            //Endpoint Routing does not support 'IApplicationBuilder.UseMvc(...)'. To use 'IApplicationBuilder.UseMvc' set 'MvcOptions.EnableEndpointRouting = false' inside 'ConfigureServices(...).
            services.AddMvc(mvcOptions => mvcOptions.EnableEndpointRouting = false);
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCookiePolicy();
            app.UseIdentityServer();
            app.UseStaticFiles();
            app.UseMvcWithDefaultRoute();
        }

        //Setup samesite cookie handling
        private void CheckSameSite(HttpContext httpContext, CookieOptions options)
        {
            if (options.SameSite == SameSiteMode.None)
            {
                var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
                if (DisallowsSameSiteNone(userAgent))
                {
                    options.SameSite = SameSiteMode.Unspecified;
                }
            }
        }

        private static bool DisallowsSameSiteNone(string userAgent)
        {
            // Cover all iOS based browsers here. This includes:
            //   - Safari on iOS 12 for iPhone, iPod Touch, iPad
            //   - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
            //   - Chrome on iOS 12 for iPhone, iPod Touch, iPad
            // All of which are broken by SameSite=None, because they use the
            // iOS networking stack.
            // Notes from Thinktecture:
            // Regarding https://caniuse.com/#search=samesite iOS versions lower
            // than 12 are not supporting SameSite at all. Starting with version 13
            // unknown values are NOT treated as strict anymore. Therefore we only
            // need to check version 12.
            if (userAgent.Contains("CPU iPhone OS 12")
               || userAgent.Contains("iPad; CPU OS 12"))
            {
                return true;
            }

            // Cover Mac OS X based browsers that use the Mac OS networking stack.
            // This includes:
            //   - Safari on Mac OS X.
            // This does not include:
            //   - Chrome on Mac OS X
            // because they do not use the Mac OS networking stack.
            // Notes from Thinktecture: 
            // Regarding https://caniuse.com/#search=samesite MacOS X versions lower
            // than 10.14 are not supporting SameSite at all. Starting with version
            // 10.15 unknown values are NOT treated as strict anymore. Therefore we
            // only need to check version 10.14.
            if (userAgent.Contains("Safari")
               && userAgent.Contains("Macintosh; Intel Mac OS X 10_14")
               && userAgent.Contains("Version/"))
            {
                return true;
            }

            // Cover Chrome 50-69, because some versions are broken by SameSite=None
            // and none in this range require it.
            // Note: this covers some pre-Chromium Edge versions,
            // but pre-Chromium Edge does not require SameSite=None.
            // Notes from Thinktecture:
            // We can not validate this assumption, but we trust Microsofts
            // evaluation. And overall not sending a SameSite value equals to the same
            // behavior as SameSite=None for these old versions anyways.
            if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
            {
                return true;
            }

            return false;
        }
    }
}
