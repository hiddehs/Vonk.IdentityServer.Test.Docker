using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using System.Net;

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
                .UseKestrel(options =>
                {
                    options.Listen(IPAddress.Loopback, 5100);
                    options.Listen(IPAddress.Loopback, 5101, listenOptions =>
                    {
                        listenOptions.UseHttps("ssl_cert.pfx", "cert-password");
                    });
                })
            .Build();
    }
}
