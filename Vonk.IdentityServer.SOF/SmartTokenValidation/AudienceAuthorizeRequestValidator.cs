using System;
using System.Threading.Tasks;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
            if (!_fhirServerConfig.Value?.FHIR_BASE_URL.Equals(audience) ?? false)
            {
                _logger.LogDebug($"AudienceAuthorizeRequestValidator - Rejecting AuthorizeRequest as the audience does not match the expected value '{_fhirServerConfig.Value?.FHIR_BASE_URL}'");
                context.Result.Error = "Invalid Authorize Request";
                context.Result.IsError = true;
            }

            return Task.CompletedTask;
        }
    }
}
