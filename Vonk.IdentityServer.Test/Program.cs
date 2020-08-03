using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
#if DEBUG
using System.Net;
#endif

namespace Vonk.IdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            BuildWebHost(args).Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseKestrel( 
#if DEBUG          
            options =>
                {
                    options.Listen(IPAddress.Loopback, 5100);
                    options.Listen(IPAddress.Loopback, 5101, listenOptions =>
                    {
                        listenOptions.UseHttps("ssl_cert.pfx", "test");
                    });
                }
#endif
                )
            .UseIIS()
            .Build();
    }
}
