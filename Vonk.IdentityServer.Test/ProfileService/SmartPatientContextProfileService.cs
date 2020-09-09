using System;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;

namespace Vonk.IdentityServer.Test.ProfileService
{
    /// <summary>
    /// IProfileService allows to provide custom identity information back to the client
    /// See http://docs.identityserver.io/en/latest/reference/profileservice.html
    /// </summary>
    public class SmartPatientContextProfileService : IProfileService
    {
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            context.IssuedClaims.Add(Config.GetDefaultPatientClaim());  // Add patient claim by default to each identity token
            context.IssuedClaims.Add(Config.GetDefaultFHIRUserClaim()); // Add fhirUser claim by default to each identity token
            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.CompletedTask;
        }
    }
}
