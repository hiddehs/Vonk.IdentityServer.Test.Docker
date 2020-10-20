using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.IO;

#if DEBUG
using System.Net;
#endif

namespace Vonk.IdentityServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
            .UseSerilog()
            .ConfigureHostConfiguration(configHost =>
            {
                // Add the default settings before the actual settings as they'll be overwritten otherwise
                configHost.SetBasePath(Directory.GetCurrentDirectory());
                configHost.AddJsonFile("appsettings.default.json", optional: true, reloadOnChange: true)
                          .AddJsonFile("appsettings.instance.json", optional: true, reloadOnChange: true)
                          .AddEnvironmentVariables("IDENTITY_SERVER_");
            })
            .ConfigureWebHostDefaults(
                webhost => {
                    webhost.UseStartup<Startup>();
                    webhost.UseKestrel(
#if DEBUG
                        options =>
                        {
                            options.Listen(IPAddress.Loopback, 5100);
                            options.Listen(IPAddress.Loopback, 5101, listenOptions =>
                            {
                                listenOptions.UseHttps("ssl_cert.pfx", "cert-password");
                            });
                        }
#endif
                   );
                   webhost.UseIIS();
            });
             
    }
}