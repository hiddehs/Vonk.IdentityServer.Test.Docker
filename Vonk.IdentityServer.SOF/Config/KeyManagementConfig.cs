using System;
namespace Vonk.IdentityServer
{
    public class KeyManagementConfig
    {
        public SigningCredentialsConfig SigningCredentialsConfig { get; set; }
    }

    public class SigningCredentialsConfig
    {
        public KeyType KeyType { get; set; }
        public Algorithm Algorithm { get; set; }
        public RSAParameterOptions RSAParameters { get; set; }
        public ECParameterOptions ECParameters { get; set; }
    }

    public class RSAParameterOptions
    {
        public string N { get; set; }
        public string E { get; set; }
        public string D { get; set; }
        public string P { get; set; }
        public string Q { get; set; }
        public string DP { get; set; }
        public string DQ { get; set; }
        public string QI { get; set; }
    }

    public class ECParameterOptions
    {
        public string X { get; set; }
        public string Y { get; set; }
        public string D { get; set; }
    }

    public enum KeyType
    {
        RSA,
        EC
    }

    public enum Algorithm
    {
        RS256,
        RS384,
        ES384
    }
}
