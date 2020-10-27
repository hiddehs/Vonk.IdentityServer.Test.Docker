using System;
using System.Collections.Generic;
using IdentityServer4.Models;

namespace Vonk.IdentityServer.SOF.Model
{
    public class SmartClient : Client
    {
        public ICollection<string> LaunchIds
        {
            get; set;
        }
    }
}
