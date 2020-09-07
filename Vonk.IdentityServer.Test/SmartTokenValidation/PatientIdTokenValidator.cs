using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Validation;
using System.Security.Claims;

namespace Vonk.IdentityServer.Test.SmartTokenValidation
{
    /// <summary>
    /// ICustomTokenRequestValidator allows to modify the access token requested by the client
    /// See http://docs.identityserver.io/en/latest/topics/custom_token_request_validation.html
    /// </summary>
    public class PatientIdTokenValidator : ICustomTokenRequestValidator
    {
        public Task ValidateAsync(CustomTokenRequestValidationContext context)
        {
            if (context.Result.CustomResponse is null)
            {
                var (type, value) = ClaimToCustomResponse(Config.GetDefaultPatientClaim());
                context.Result.CustomResponse = new Dictionary<string, object>();
                context.Result.CustomResponse.Add(type, value);
            }
            else
            {
                var (type, value) = ClaimToCustomResponse(Config.GetDefaultPatientClaim());
                context.Result.CustomResponse.Add(type, value);
            }

            return Task.CompletedTask;
        }

        private (string, object) ClaimToCustomResponse(Claim claim)
        {
            var defaultClaim = Config.GetDefaultPatientClaim();
            return (defaultClaim.Type, defaultClaim.Value);
        }
    }
}
