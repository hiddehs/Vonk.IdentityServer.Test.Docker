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
    /// This ICustomTokenRequestValidator will add the patient and launch context information to the access token
    /// </summary>
    public class SmartAccessTokenValidator : ICustomTokenRequestValidator
    {
        public Task ValidateAsync(CustomTokenRequestValidationContext context)
        {
            if (context.Result.CustomResponse is null)
            {
                context.Result.CustomResponse = new Dictionary<string, object>();

                var (type, value) = ClaimToCustomResponse(Config.GetDefaultPatientClaim());
                context.Result.CustomResponse.Add(type, value);

                (type, value) = ClaimToCustomResponse(Config.GetDefaultNeedPatientBanner());
                context.Result.CustomResponse.Add(type, value);

                (type, value) = ClaimToCustomResponse(Config.GetDefaultStyleUrl());
                context.Result.CustomResponse.Add(type, value);
            }
            else
            {
                throw new InvalidOperationException("CustomTokenRequestValidationContext - Unexpected CustomResponse was included included in validation context");
            }

            return Task.CompletedTask;
        }

        private (string, object) ClaimToCustomResponse(Claim claim)
        {
            return (claim.Type, claim.Value);
        }
    }
}
