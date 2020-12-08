using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Threading.Tasks;
using IdentityServer4.Models;
using IdentityServer4.Stores;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Vonk.IdentityServer.Test.Support;

namespace Vonk.IdentityServer.SOF.KeyManagement
{
    public class SmartKeyStore : ISigningCredentialStore, IValidationKeysStore
    {
        private readonly IOptions<KeyManagementConfig> _signingCredentialsOptions;

        public SmartKeyStore(IOptions<KeyManagementConfig> options)
        {
            Check.NotNull(options, nameof(options));
            _signingCredentialsOptions = options;
        }

        public Task<SigningCredentials> GetSigningCredentialsAsync()
        {
            var signingCredentialsConfig = _signingCredentialsOptions.Value?.SigningCredentialsConfig;

            var key = BuildSecurityKey();
            var algorithm = TranslateAlgorithm(signingCredentialsConfig?.Algorithm);

            return Task.FromResult(new SigningCredentials(key, algorithm));
        }

        public Task<IEnumerable<SecurityKeyInfo>> GetValidationKeysAsync()
        {
            var signingCredentialsConfig = _signingCredentialsOptions.Value?.SigningCredentialsConfig;

            var keyInfo = new SecurityKeyInfo
            {
                Key = BuildSecurityKey(),
                SigningAlgorithm = TranslateAlgorithm(signingCredentialsConfig?.Algorithm)
            };

            return Task.FromResult(new[] { keyInfo } as IEnumerable<SecurityKeyInfo>);
        }

        private SecurityKey BuildSecurityKey()
        {
            var signingCredentialsConfig = _signingCredentialsOptions.Value?.SigningCredentialsConfig;
            var keyType = signingCredentialsConfig?.KeyType;
            var algorithm = signingCredentialsConfig?.Algorithm;

            var key = (keyType, algorithm) switch
            {
                (KeyType.RSA, Algorithm.RS256) => BuildRSAKey(signingCredentialsConfig?.RSAParameters),
                (KeyType.RSA, Algorithm.RS384) => BuildRSAKey(signingCredentialsConfig?.RSAParameters),
                (KeyType.EC, Algorithm.ES384) => BuildECKey(signingCredentialsConfig?.ECParameters, signingCredentialsConfig?.Algorithm),
                _ => throw new NotImplementedException($"SigningCredentialStore - KeyType '{keyType}' and Algorithm '{algorithm}' do not match in SigningCredentialsConfig")
            };

            return key;
        }

        private SecurityKey BuildECKey(ECParameterOptions ecParameters, Algorithm? algorithm)
        {
            if (ecParameters?.X is null   ||
                ecParameters?.Y is null   ||
                ecParameters?.D is null)
                throw new InvalidOperationException("SigningCredentialStore - ECParameter options are incomplete, mandatory parameter(s) are missing");

            var ecdsa = ECDsa.Create(new ECParameters
            {
                Curve = TranslateCurve(algorithm),
                D = base64urldecode(ecParameters.D),
                Q = new ECPoint { X = base64urldecode(ecParameters.X), Y = base64urldecode(ecParameters.Y) }
            });

            var key = new ECDsaSecurityKey(ecdsa);

            return key;
        }

        private SecurityKey BuildRSAKey(RSAParameterOptions rsaParameters)
        {
            if (rsaParameters?.D is null ||
                rsaParameters?.E is null ||
                rsaParameters?.N is null ||
                rsaParameters?.P is null ||
                rsaParameters?.Q is null ||
                rsaParameters?.DP is null ||
                rsaParameters?.DQ is null ||
                rsaParameters?.QI is null)
                throw new InvalidOperationException("SigningCredentialStore - RSAParameter options are incomplete, mandatory parameter(s) are missing");

            var rsa = RSA.Create(new RSAParameters
            {
                D = base64urldecode(rsaParameters.D),
                Exponent = base64urldecode(rsaParameters.E),
                Modulus = base64urldecode(rsaParameters.N),
                P = base64urldecode(rsaParameters.P),
                Q = base64urldecode(rsaParameters.Q),
                DP = base64urldecode(rsaParameters.DP),
                DQ = base64urldecode(rsaParameters.DQ),
                InverseQ = base64urldecode(rsaParameters.QI),
            });

            var key = new RsaSecurityKey(rsa);

            return key;
        }

        private string TranslateAlgorithm(Algorithm? algorithm)
        {
            switch (algorithm)
            {
                case Algorithm.RS256:
                    return SecurityAlgorithms.RsaSha256;
                case Algorithm.RS384:
                    return SecurityAlgorithms.RsaSha384;
                case Algorithm.ES384:
                    return SecurityAlgorithms.EcdsaSha384;
                default:
                    throw new InvalidOperationException($"SigningCredentialStore - Found unsupported algorithm '{algorithm}'");
            }
        }

        // Workaround: Using ECCurve.CreateFromFriendlyName results in a PlatformException for NIST curves
        private ECCurve TranslateCurve(Algorithm? algorithm)
        {
            switch (algorithm)
            {
                case Algorithm.ES384:
                    return ECCurve.CreateFromOid(new Oid("1.3.132.0.34"));
                default:
                    throw new InvalidOperationException($"SigningCredentialStore - Cannot create curve for algorithm '{algorithm}'");
            }
        }
        // See https://tools.ietf.org/html/rfc7515#appendix-C
        static byte[] base64urldecode(string arg)
        {
            string s = arg;
            s = s.Replace('-', '+'); // 62nd char of encoding
            s = s.Replace('_', '/'); // 63rd char of encoding
            switch (s.Length % 4) // Pad with trailing '='s
            {
                case 0: break; // No pad chars in this case
                case 2: s += "=="; break; // Two pad chars
                case 3: s += "="; break; // One pad char
                default:
                    throw new System.Exception("Illegal base64url string!");
            }
            return Convert.FromBase64String(s); // Standard base64 decoder
        }
    }

}
