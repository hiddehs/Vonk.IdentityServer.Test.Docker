using IdentityServer4;
using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using Vonk.IdentityServer.SOF.Model;
using F = Hl7.Fhir.Model;

namespace Vonk.IdentityServer
{
    public class Config
    {

        #region ApiScopes

        public static List<ApiScope> GetApiScopes() =>
          (
            from resourceType in F.ModelInfo.SupportedResources.Union(new[] { "*" })
            from subject in new[] { "user", "patient", "system" }
            from action in new[] { "read", "write" }
            select
                new ApiScope
                {
                    Name = $"{subject}/{resourceType}.{action}",
                    DisplayName = $"SMART on FHIR - {subject} may {action} resources of type {resourceType}"
                }
            )
            .Union(new[]
            {
                new ApiScope{
                    Name = "launch",
                    DisplayName = "Permission to obtain launch context when app is launched from an EHR",
                    UserClaims = new[] {"patient", "encounter", "location" }
                },
                new ApiScope
                {
                    Name = "launch/patient",
                    DisplayName = "When launching outside the EHR, ask for a patient to be selected at launch time"
                },
                new ApiScope{
                    Name = "fhirUser",
                    DisplayName = "Permission to retrieve information about the current logged-in user"
                }
            })
            .ToList();

        #endregion ApiScopes

        #region ApiResources

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource{
                    Name = "vonk",
                    DisplayName = "Vonk FHIR Server",
                    Scopes = GetApiScopes().Select(scope => scope.Name).ToList()
                }
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
            };
        }

        #endregion ApiResources

        #region Clients

        public static IEnumerable<Client> GetClients()
        {

            return new List<Client>
            {
                new Client
                {
                    ClientId = "Postman",
                    RedirectUris = new[] {"https://www.getpostman.com/oauth2/callback", "https://oauth.pstmn.io/v1/callback" },

                    AllowedGrantTypes = GrantTypes.Code,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes = GetApiScopes().Select(scope => scope.Name).Union(new[] { "openid", "profile" }).ToList(),
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RequirePkce = false, // Allow as an interactive client
                },
                new SmartClient
                {
                    RequireConsent = true, // The user (not the requesting SMART app) needs to be able to disallow access to certain resource types, specific claims can be de-selected on the consent page
                    ClientId = "Inferno",
                    RedirectUris = new[] { "http://0.0.0.0:4567/inferno/oauth2/static/redirect",
                                           "http://localhost:4567/inferno/oauth2/static/redirect",
                                           "http://vonkhost:4567/inferno/oauth2/static/redirect",
                                           "https://inferno.healthit.gov/inferno/oauth2/static/redirect"},

                    AllowedGrantTypes = GrantTypes.Code,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes = GetApiScopes().Select(scope => scope.Name).Union(new[] { "openid", "profile" }).ToList(),
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RequirePkce = false, // Allow as an interactive client
                    AllowOfflineAccess = true,
                    LaunchIds = new[] { "xyz123" } // Launch ids are opaque identifiers passed to the identity server to communicate the EHR context. Currently hard-coded as there is no communication with external systems, i.e. EHRs, at the moment.
                },
                new Client
                {
                    RequireConsent = true, // The user (not the requesting SMART app) needs to be able to disallow access to certain resource types, specific claims can be de-selected on the consent page
                    ClientId = "Inferno-Public",
                    RedirectUris = new[] { "http://0.0.0.0:4567/inferno/oauth2/static/redirect",
                                           "http://localhost:4567/inferno/oauth2/static/redirect",
                                           "http://vonkhost:4567/inferno/oauth2/static/redirect",
                                           "https://inferno.healthit.gov/inferno/oauth2/static/redirect"},

                    AllowedGrantTypes = GrantTypes.Code,

                    // no secret for authentication

                    // scopes that client has access to
                    AllowedScopes = GetApiScopes().Select(scope => scope.Name).Union(new[] { "openid", "profile" }).ToList(),
                    AlwaysIncludeUserClaimsInIdToken = true,
                    RequirePkce = false, // Allow as an interactive client
                    AllowOfflineAccess = true,
                    RequireClientSecret = false
                },
                new Client
                {
                    ClientId = "Inferno-Bulk",
                    RedirectUris = new[] { "http://0.0.0.0:4567/inferno/oauth2/static/redirect",
                                           "http://localhost:4567/inferno/oauth2/static/redirect",
                                           "http://vonkhost:4567/inferno/oauth2/static/redirect",
                                           "https://inferno.healthit.gov/inferno/oauth2/static/redirect"},
                    ClientSecrets =
                    {
                        new Secret
                        {
                            Type = IdentityServerConstants.SecretTypes.JsonWebKey,
                            Value = "{'e':'AQAB','kid':'4b49a739d1eb115b3225f4cf9beb6d1b','kty':'RSA','n':'vjbIzTqiY8K8zApeNng5ekNNIxJfXAue9BjoMrZ9Qy9m7yIA-tf6muEupEXWhq70tC7vIGLqJJ4O8m7yiH8H2qklX2mCAMg3xG3nbykY2X7JXtW9P8VIdG0sAMt5aZQnUGCgSS3n0qaooGn2LUlTGIR88Qi-4Nrao9_3Ki3UCiICeCiAE224jGCg0OlQU6qj2gEB3o-DWJFlG_dz1y-Mxo5ivaeM0vWuodjDrp-aiabJcSF_dx26sdC9dZdBKXFDq0t19I9S9AyGpGDJwzGRtWHY6LsskNHLvo8Zb5AsJ9eRZKpnh30SYBZI9WHtzU85M9WQqdScR69Vyp-6Uhfbvw'}"
                        }
                    },
                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials,
                    AllowedScopes = GetApiScopes().Where(scope => scope.Name.StartsWith("system")).Select(scope => scope.Name).ToList(), // openid and profile are not needed here because we are dealing with system scopes here
                }
            };
        }

        #endregion Clients

        #region Users

        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "alice",
                    Password = "password",
                    Claims = new List<Claim>() { GetDefaultPatientClaim() }
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "bob",
                    Password = "password",
                    Claims = new List<Claim>() { GetDefaultPatientClaim() }
                }
            };
        }

        #endregion Users

        #region Claims

        // See http://www.hl7.org/fhir/smart-app-launch/scopes-and-launch-context/index.html#launch-context-arrives-with-your-access_token
        // In case of a standalone lauch, the patient context needs to be selected based on the patient authorization information (user needs to correspond to a single Patient resource)
        // In case of an EHR launch, the patient context is provided by the EHR using the lauch context
        // For now we hard-code the launch context to a fixed claim in the access token
        public static Claim GetDefaultPatientClaim()
        {
            return new Claim("patient", "test");
        }

        // An identity token may be requested together with the access token
        // If the fhirUser scope is requested, an URL pointing to a Patient / Practitioner / RelatedPerson / Person resource should be included
        // See http://www.hl7.org/fhir/smart-app-launch/scopes-and-launch-context/index.html#scopes-for-requesting-identity-data
        // For now we hard-code the fhirUser to a fixed claim in the id token
        public static Claim GetDefaultFHIRUserClaim(string fhirBaseUrl)
        {
            return new Claim("fhirUser", $"{fhirBaseUrl}/Practitioner/test");
        }

        // Boolean value indicating whether the app was launched in a UX context where a patient banner is required (when true) or not required (when false).
        // An app receiving a value of false should not take up screen real estate displaying a patient banner.
        public static Claim GetDefaultNeedPatientBanner()
        {
            return new Claim("need_patient_banner", "false");
        }

        // String URL where the host’s style parameters can be retrieved (for apps that support styling)
        public static Claim GetDefaultStyleUrl(string identityServerBaseUrl)
        {
            return new Claim("smart_style_url", $"{identityServerBaseUrl}/smart/style/v1.json");
        }

        #endregion Claims

        #region IdentityServerOptions

        public static void GetOptions(IdentityServerOptions identityServerOptions)
        {
            identityServerOptions.InputLengthRestrictions.Scope = 5000; // 149 resources in FHIR R4 * 30 characters
        }

        #endregion IdentityServerOptions

    }
}
