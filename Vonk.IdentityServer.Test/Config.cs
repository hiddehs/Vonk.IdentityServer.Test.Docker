using IdentityServer4.Configuration;
using IdentityServer4.Models;
using IdentityServer4.Test;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using F = Hl7.Fhir.Model;

namespace Vonk.IdentityServer
{
    public class Config
    {

        #region ApiScopes

        public static List<ApiScope> GetApiScopes() =>
          (
            from resourceType in F.ModelInfo.SupportedResources.Union(new[] { "*" })
            from subject in new[] { "user", "patient" }
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
                    RequirePkce = false // Allow as an interactive client
                },
                new Client
                {
                    RequireConsent = true, // The user (not the requesting SMART app) needs to be able to disallow access to certain resource types, specific claims can be de-selected on the consent page
                    ClientId = "Inferno",
                    RedirectUris = new[] { "http://0.0.0.0:4567/inferno/oauth2/static/redirect", "http://localhost:4567/inferno/oauth2/static/redirect", "http://vonkhost:4567/inferno/oauth2/static/redirect" },

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
                    AllowOfflineAccess = true
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

        public static Claim GetDefaultPatientClaim()
        {
            return new Claim("patient", "test");
        }

        public static Claim GetDefaultFHIRUserClaim()
        {
            return new Claim("fhirUser", $"{FHIR_BASE}/Practitioner/test");
        }

        #endregion Claims

        #region IdentityServerOptions

        public static void GetOptions(IdentityServerOptions identityServerOptions)
        {
            identityServerOptions.InputLengthRestrictions.Scope = 5000; // 149 resources in FHIR R4 * 30 characters
        }

        private readonly static string FHIR_BASE = "http://vonkhost" +
            ":4080"; //"https://vonk.fire.ly";

        #endregion IdentityServerOptions

    }
}
