using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Options;
using Vonk.IdentityServer.Test.Support;

namespace Vonk.IdentityServer.Test.ProfileService
{
    /// <summary>
    /// IProfileService allows to provide custom identity information back to the client
    /// See http://docs.identityserver.io/en/latest/reference/profileservice.html
    /// </summary>
    public class SmartPatientContextProfileService : IProfileService
    {
        private readonly IOptions<FHIRServerConfig> _fhirServerConfig;

        public SmartPatientContextProfileService(IOptions<FHIRServerConfig> fhirServerConfig)
        {
            Check.NotNull(fhirServerConfig, nameof(fhirServerConfig));
            _fhirServerConfig = fhirServerConfig;
        }

        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            Check.NotNull(context, nameof(context));

            // ToDo: Check if these scopes are requested in context.RequestedResources

            context.IssuedClaims.Add(Config.GetDefaultPatientClaim()); // Need to be added, otherwise Vonk can't enforce the Patient compartment
            context.IssuedClaims.Add(Config.GetDefaultFHIRUserClaim(_fhirServerConfig.Value?.FHIR_BASE_URL));

            return Task.CompletedTask;
        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            return Task.CompletedTask;
        }
    }
}
