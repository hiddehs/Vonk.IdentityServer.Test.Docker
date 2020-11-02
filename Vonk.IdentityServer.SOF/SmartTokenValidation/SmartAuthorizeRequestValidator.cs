using System;
using System.Linq;
using System.Threading.Tasks;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Vonk.IdentityServer.SOF.Model;
using Vonk.IdentityServer.Test.Support;

namespace Vonk.IdentityServer.SOF.SmartTokenValidation
{
    public class AudienceAuthorizeRequestValidator : ICustomAuthorizeRequestValidator
    {
        private readonly IOptions<FHIRServerConfig> _fhirServerConfig;
        private readonly ILogger<AudienceAuthorizeRequestValidator> _logger;

        public AudienceAuthorizeRequestValidator(IOptions<FHIRServerConfig> fhirServerConfig, ILogger<AudienceAuthorizeRequestValidator> logger)
        {
            Check.NotNull(fhirServerConfig, nameof(fhirServerConfig));
            Check.NotNull(logger, nameof(logger));
            _fhirServerConfig = fhirServerConfig;
            _logger = logger;
        }

        public Task ValidateAsync(CustomAuthorizeRequestValidationContext context)
        {
            Check.NotNull(context, nameof(context));

            var audience = context.Result.ValidatedRequest.Raw["aud"];
            if (audience is { } && (!_fhirServerConfig.Value?.FHIR_BASE_URL.Equals(audience) ?? false))
            {
                _logger.LogDebug($"AudienceAuthorizeRequestValidator - Rejecting AuthorizeRequest as the audience '{audience}' does not match the expected value '{_fhirServerConfig.Value?.FHIR_BASE_URL}'");
                context.Result.Error = "Invalid Authorize Request";
                context.Result.IsError = true;
            }

            var clientId = context.Result.ValidatedRequest.Raw["client_id"];
            var match = Config.GetClients().Where(c => c.ClientId.Equals(clientId));
            var smartClient = match.FirstOrDefault() as SmartClient;

            var launchId = context.Result.ValidatedRequest.Raw["launch"];
            if(smartClient is { } && launchId is { } && !smartClient.LaunchIds.Contains(launchId))
            {
                _logger.LogDebug($"AudienceAuthorizeRequestValidator - Rejecting AuthorizeRequest as the launch id '{launchId}' does not match the expected values '{string.Join(',', smartClient.LaunchIds)}'");
                context.Result.Error = "Invalid Authorize Request";
                context.Result.IsError = true;
            }

            return Task.CompletedTask;
        }
    }
}
