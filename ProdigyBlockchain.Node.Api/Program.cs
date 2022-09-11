using Prodigy.BusinessLayer.Networks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Net;

namespace Prodigy.Node.Api
{
    public class NodeApiProgram
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
        }

        public static IHostBuilder CreateHostBuilder(string[] args, IPAddress ip_address, INetwork network)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls("http://localhost:" + network.DefaultAPIPort, "http://" + ip_address + ":" + network.DefaultAPIPort);
                });
        }
            
    }
}
