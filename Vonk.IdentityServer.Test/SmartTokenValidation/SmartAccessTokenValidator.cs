using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IdentityServer4.Validation;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Vonk.IdentityServer.Test.Support;

namespace Vonk.IdentityServer.Test.SmartTokenValidation
{
    /// <summary>
    /// ICustomTokenRequestValidator allows to modify the access token requested by the client
    /// See http://docs.identityserver.io/en/latest/topics/custom_token_request_validation.html
    /// This ICustomTokenRequestValidator will add the patient and launch context information to the access token
    /// </summary>
    public class SmartAccessTokenValidator : ICustomTokenRequestValidator
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SmartAccessTokenValidator(IHttpContextAccessor httpContextAccessor)
        {
            Check.NotNull(httpContextAccessor, nameof(httpContextAccessor));
            _httpContextAccessor = httpContextAccessor;
        }

        public Task ValidateAsync(CustomTokenRequestValidationContext context)
        {
            if (context.Result.CustomResponse is null)
            {
                context.Result.CustomResponse = new Dictionary<string, object>();

                var (type, value) = ClaimToCustomResponse(Config.GetDefaultPatientClaim());
                context.Result.CustomResponse.Add(type, value);

                (type, value) = ClaimToCustomResponse(Config.GetDefaultNeedPatientBanner());
                context.Result.CustomResponse.Add(type, value);

                var httpContext = _httpContextAccessor.HttpContext;
                var baseUrl = IdentitysServerBaseUrl(httpContext);

                (type, value) = ClaimToCustomResponse(Config.GetDefaultStyleUrl(baseUrl));
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

        private string IdentitysServerBaseUrl(HttpContext httpContext)
        {
            return $"{httpContext.Request.Scheme}://{httpContext.Connection.LocalIpAddress}:{httpContext.Connection.LocalPort}";
        }
    }
}
