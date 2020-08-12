using System;
namespace Vonk.IdentityServer.Test.Support
{
    public static class Check
    {
        public static void NotNull(object toCheck, string parametername = null, string message = null)
        {
            if (toCheck == null)
                throw new ArgumentNullException(parametername, message);
        }
    }
}
