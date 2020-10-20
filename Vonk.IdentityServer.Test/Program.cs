using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using static Microsoft.Extensions.Logging.AzureAppServicesLoggerFactoryExtensions;
using Serilog;
using Serilog.Events;
using System.IO;
using System;

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
            .WriteTo.File( // Write to Azure Log Stream
                @"D:\home\LogFiles\Application\identity_server_log.txt",
                fileSizeLimitBytes: 1_000_000,
                rollOnFileSizeLimit: true,
                shared: true,
                flushToDiskInterval: TimeSpan.FromSeconds(1))
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
                            options.Listen(IPAddress.Parse("192.168.178.30"), 5100);
                            options.Listen(IPAddress.Loopback, 5101, listenOptions =>
                            {
                                listenOptions.UseHttps("ssl_cert.pfx", "test");
                            });
                        }
#endif
                   );
                   webhost.UseIIS();
            });
             
    }
}