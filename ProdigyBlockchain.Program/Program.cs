using Prodigy.BusinessLayer;
using Prodigy.BusinessLayer.CLI;
using Prodigy.BusinessLayer.Services;
using Prodigy.Node.Api;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
namespace Prodigy.Node
{
    class Program
    {
        private static ProdigyNode _Node;
        private static IPAddress _ExternalIPAddress;
        private static IPAddress _InternalIPAddress;

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("Starting Node");
            Console.WriteLine("Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());

            // Load the config
            var config = new ConfigurationBuilder().AddJsonFile("config.json", optional: false).Build();
            var _NodeConfig = new NodeConfig();
            var _WalletConfig = new WalletSettings();

            config.GetSection("node").Bind(_NodeConfig);
            config.GetSection("wallet").Bind(_WalletConfig);

            // Get the IP addresses
            GetExternalIP();
            GetInternalIP();

            // Configure base node functons
            if (_NodeConfig.network_id == 1)
                _Node = new ProdigyNode(_NodeConfig, _WalletConfig, _ExternalIPAddress, NetworkType.Testnet);
            else if (_NodeConfig.network_id == 2)
                _Node = new ProdigyNode(_NodeConfig, _WalletConfig, _ExternalIPAddress, NetworkType.Mainnet);
            else
                throw new Exception("Wrong network");

            //_Node.LoadFromOldSystem();


            Console.WriteLine("Joining network...");

            Task.Run(() => {
                Thread.Sleep(5000);
                var join_result = _Node.RequestToJoinNetwork();

                if(join_result == true)
                {
                    try
                    {
                        Thread.Sleep(1000);
                        _Node.DownloadBlockchainFromNodes();
                        StartAPILayer();

                        
                    }
                    catch (Exception)
                    {
                        // Can't continue without blocks
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Can't continue without blocks. Exiting application.");
                        Console.ForegroundColor = ConsoleColor.White;
                        Environment.Exit(0);
                    }
                }
                else
                {
                    // Can't continue without blocks
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Could not join network");
                    Console.ForegroundColor = ConsoleColor.White;
                    Environment.Exit(0);
                }
            });


            var start = new CLI(_Node);


        }

        private static void StartAPILayer()
        {
            Console.WriteLine("Starting API...");
            NodeApiProgram.CreateHostBuilder(null, _InternalIPAddress, _Node.Network)
                .ConfigureServices(servicesCollection =>
                {
                    servicesCollection.AddSingleton<IBlockchainDataService>(_Node.BlockchainDataService);
                    servicesCollection.AddSingleton<INodeDataService>(_Node.NodeDataService);
                    servicesCollection.AddSingleton<IWalletDataService>(_Node.WalletDataService);
                })
                .Build()
                .Run();
        }

        private static void GetExternalIP()
        {
            string externalIpString = new WebClient().DownloadString("http://icanhazip.com").Replace("\\r\\n", "").Replace("\\n", "").Trim();
            _ExternalIPAddress = IPAddress.Parse(externalIpString);
        }

        private static void GetInternalIP()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            _InternalIPAddress = host.AddressList.Where(m => m.AddressFamily == AddressFamily.InterNetwork).LastOrDefault();
            // Linux
            if (_InternalIPAddress == null)
            {
                _InternalIPAddress = NetworkInterface.GetAllNetworkInterfaces().SelectMany(i => i.GetIPProperties().UnicastAddresses).Select(a => a.Address).Where(a => a.AddressFamily == AddressFamily.InterNetwork).LastOrDefault();
            }
            else
            {
                var ipAddresses = host.AddressList.Where(m => m.AddressFamily == AddressFamily.InterNetwork).ToList();
                foreach (var theIP in ipAddresses)
                {
                    if (theIP.ToString().Contains("192."))
                    {
                        _InternalIPAddress = theIP;
                    }
                }
            }

            Console.WriteLine("Binding to: " + _InternalIPAddress.ToString());
        }
    }
}
