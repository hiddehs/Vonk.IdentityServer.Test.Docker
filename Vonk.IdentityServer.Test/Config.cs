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
        private static List<Scope> _scopes =
            (
                from resourceType in F.ModelInfo.SupportedResources.Union(new[] { "*" })
                from subject in new[] { "user", "patient" }
                from action in new[] { "read", "write" }
                select
                 new Scope
                 {
                     Name = $"{subject}/{resourceType}.{action}",
                     DisplayName = $"SMART on FHIR - {subject} may {action} resources of type {resourceType}"
                 }
            )
            .Union(new[]
            {
                new Scope{
                    Name = "launch",
                    DisplayName = "SMART on FHIR launch context",
                    UserClaims = new[] {"patient", "encounter", "location" }
                },
            })
            .ToList();

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource{
                    Name = "vonk",
                    DisplayName = "Vonk FHIR Server",
                    Scopes = _scopes
                }
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "Postman",
                    RedirectUris = new[] {"https://www.getpostman.com/oauth2/callback" },

                    AllowedGrantTypes = GrantTypes.Code,

                    // secret for authentication
                    ClientSecrets =
                    {
                        new Secret("secret".Sha256())
                    },

                    // scopes that client has access to
                    AllowedScopes = _scopes.Select(scope => scope.Name).Union(new[] { "openid", "profile" }).ToList(),
                    AlwaysIncludeUserClaimsInIdToken = true,
                },
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

        public static List<TestUser> GetUsers()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "alice",
                    Password = "password"
                    , Claims = new List<Claim>() { new Claim("patient", "alice-identifier")}
                },
                new TestUser
                {
                    SubjectId = "2",
                    Username = "bob",
                    Password = "password"
                    , Claims = new List<Claim>() { new Claim("patient", "bob-identifier")}
                }
            };
        }

    }
}
