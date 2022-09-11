using Prodigy.BusinessLayer.Blockchain;
using Prodigy.BusinessLayer.Events;
using Prodigy.BusinessLayer.Models;
using Prodigy.BusinessLayer.Models.Command;
using Prodigy.BusinessLayer.Networks;
using Prodigy.BusinessLayer.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Timers;

namespace Prodigy.BusinessLayer
{
    public class ProdigyNode
    {
        private IPAddress _NodeAddress;
        private INetwork _Network;
        private Guid _NodeId;
        private INodeConfig _Config;
        private IWalletSettings _WalletSettings;

        private IDatabaseConnectionSettings _BlockchainDatabaseSettings;
        private IDatabaseConnectionSettings _CertsDatabaseSettings;

        private IBlockchainContext _BlockchainContext;
        private ICertContext _CertContext;

        private IDocumentBlockchain _Blockchain;

        private CryptoService _CryptoService;
        private TransactionPool _TransactionPool;

        private IBlockchainDataService _BlockchainDataService;
        private INodeDataService _NodeDataService;
        private DatabaseDataService _DatabaseDataService;
        private IWalletDataService _IWalletDataService;

        private Timer _NodePingTimer;

        //////////////////////////////////////////////////////////////////////////
        // Public properties
        public IDocumentBlockchain Blockchain { get { return _Blockchain; } }
        public INetwork Network { get { return _Network; } }
        public IBlockchainDataService BlockchainDataService { get { return _BlockchainDataService; } }
        public INodeDataService NodeDataService { get { return _NodeDataService; } }
        public IWalletDataService WalletDataService { get { return _IWalletDataService; } }

        public ProdigyNode(INodeConfig nodeConfig, IWalletSettings walletSettings, IPAddress node_ip, NetworkType type)
        {
            _Config = nodeConfig;
            _NodeAddress = node_ip;
            _WalletSettings = walletSettings;
            _NodeId = Guid.Parse(nodeConfig.node_id);

            SetupConnections(type);


            // Database contexts
            _BlockchainContext = new BlockchainContext(_BlockchainDatabaseSettings);
            _CertContext = new CertContext(_CertsDatabaseSettings);

            // General Services
            _CryptoService = new(_Network.NodePrivateKey, _WalletSettings);
            _TransactionPool = new TransactionPool();

            // Blockchains
            _Blockchain = new DocumentBlockchain(_Network, nodeConfig, _CryptoService, _TransactionPool);

            //Events
            _Blockchain.NewBlockAvailable += _Blockchain_NewBlockAvailable;
            _Blockchain.BlockMined += _Blockchain_BlockMined;
            _Blockchain.BlockValidated += _Blockchain_BlockValidated;

            // Data Services
            _DatabaseDataService = new DatabaseDataService(nodeConfig, _BlockchainContext, nodeConfig.db_directory, _Network.NodePrivateKey);
            _BlockchainDataService = new BlockchainDataService(_Network, nodeConfig, _CertContext, _DatabaseDataService, _Blockchain, _Network.NodePrivateKey);
            _NodeDataService = new NodeDataService(_NodeId, _Network, _NodeAddress, _Blockchain, _Network.NodePrivateKey);
            _IWalletDataService = new WalletDataService(_Network, _WalletSettings, _Blockchain, _CryptoService);

            // Timers
            _NodePingTimer = new Timer(60000);
            _NodePingTimer.Elapsed += _NodePingTimer_Elapsed;
            _NodePingTimer.Enabled = true;

            // Tests
            //_Blockchain.StressTestBlockchain();
            //_DatabaseDataService.FullTestDatabase();

            // Load data if it exists
            LoadDatabase();

            //var derp = _BlockchainDataService.GetBlockById(Guid.Parse("c24aba17-3e3e-46b5-a998-3dcc49c659d2")).Result;
            //string ss = "";


            //var data = File.ReadAllBytes(@"C:\Users\ryan-\Downloads\Reference.pdf");

            //var command = new DocumentCreateCommand()
            //{
            //    comapny_id = Guid.NewGuid(),
            //    identifier1 = "123123",
            //    identifier2 = "123123",
            //    identifier3 = "document",
            //    document_base64_data = System.Convert.ToBase64String(data),
            //};

           // _NodeDataService.CertCreated(command);

        }


        /// <summary>
        /// Loads block data from the file system or S3 object storage
        /// </summary>
        private void LoadDatabase()
        {
            var size = _DatabaseDataService.GetBlockSize();
            Console.WriteLine("Loading: " + size);

            for(int i = 1; i <= size; i++)
            {
                var block = _DatabaseDataService.GetBlock(i).Result;

                _Blockchain.LoadFromDatabase(block);

                Console.WriteLine(i + "/" + size);
            }
        }


        /// <summary>
        /// Contacts a seed node and attempts to handshake
        /// </summary>
        /// <returns>True if this node could establish a connection to a seed node</returns>
        public bool RequestToJoinNetwork()
        {
            return _NodeDataService.JoinNetwork();
        }


        /// <summary>
        /// Attempts to download all blockchain data from an existing node
        /// </summary>
        public void DownloadBlockchainFromNodes()
        {
            Console.WriteLine("Downloading blocks...");

            _BlockchainDataService.DownloadBlockchain_Outbound();
        }


        /// <summary>
        /// Sets up connection to SQLLite database for key storage, logging database, and mass importation database
        /// </summary>
        /// <param name="node_id">Node id</param>
        /// <param name="type">Network Type</param>
        private void SetupConnections(NetworkType type)
        {
            List<string> ip_addresses = new List<string>();

            // Checkling for ip seeds and setting up
            if (String.IsNullOrEmpty(_Config.ip_seeds))
                throw new Exception("Error seting up network. The ip_seeds variable in the config is empty. Please include at least one public node IP address.");

            var ips = _Config.ip_seeds.Split(",");
            foreach(var ip in ips)
            {
                ip_addresses.Add(ip.Trim());
            }



            // Set up settings for main net
            if (type == NetworkType.Mainnet)
            {
                _Network = new MainNetwork(_NodeId, _Config.ticker, ip_addresses);

                // NOTE
                // Unless you are customizing how data is joined or saving data offsite you will most likily not need this database setting configured. As of now it does nothing so leaving as is will be okay.
                _CertsDatabaseSettings = new DatabaseConnectionSettings()
                {
                    ConnectionString = "server=YOUR_DATABASE_SERVER;user=YOUR_USER;password=YOUR_PASSWORD;database=YOUR_DATABASE;convert zero datetime=True;TreatTinyAsBoolean=True;SslMode=none"
                };

                _BlockchainDatabaseSettings = new DatabaseConnectionSettings()
                {
                    ConnectionString = "Data Source=BlockKeys.db;"
                };
            }
            else
            {
                _Network = new TestNetwork(_NodeId, _Config.ticker, ip_addresses);

                // NOTE
                // Unless you are customizing how data is joined or saving data offsite you will most likily not need this database setting configured. As of now it does nothing so leaving as is will be okay.
                _CertsDatabaseSettings = new DatabaseConnectionSettings()
                {
                    ConnectionString = "server=YOUR_DATABASE_SERVER;user=YOUR_USER;password=YOUR_PASSWORD;database=YOUR_DATABASE;convert zero datetime=True;TreatTinyAsBoolean=True;SslMode=none"
                };

                _BlockchainDatabaseSettings = new DatabaseConnectionSettings()
                {
                    ConnectionString = "Data Source=BlockKeys.db;"
                };
            }

        }


        /// <summary>
        /// Sets the genesis block from the CLI
        /// </summary>
        /// <param name="block_id">Block Id</param>
        public void SetGenesisBlock(Guid block_id)
        {
            _Blockchain.SetGenesisBlock(block_id);
        }

        public async Task<bool> PerformAudit()
        {
            var blocks_size = _Blockchain.Count();
            var first_block = _Blockchain.FirstOrDefault();

            if (first_block != null)
            {
                for (int i = 1; i < blocks_size; i++)
                {
                    var currentBlock = _Blockchain.GetByIndex(i);
                    var previousBlock = _Blockchain.GetByIndex(i - 1);

                    var full_block = await _DatabaseDataService.GetBlock(currentBlock.BlockId);

                    var rehashed_value = full_block.ValidateHash(_Network.Difficulty);

                    if (currentBlock.Hash != rehashed_value)
                    {
                        Console.WriteLine("Blockchain is invalid.");
                        return false;
                    }

                    if (currentBlock.PreviousHash != previousBlock.Hash)
                    {
                        Console.WriteLine("Blockchain is invalid.");
                        return false;
                    }
                }
            }
            else
            {
                Console.WriteLine("Could not find first block.");
                return false;
            }

            return true;
        }


        /////////////////////////////////////////////////////////////////////////////////////////
        //Events
        private void _Blockchain_NewBlockAvailable(object sender, BlockchainEventArgs e)
        {
            Console.WriteLine("New block available... Letting others know.");
            _BlockchainDataService.BlockAdded_Outbound(e.Block);
        }

        private void _Blockchain_BlockMined(object sender, BlockchainMiningEventArgs e)
        {
            
        }

        private void _Blockchain_BlockValidated(object sender, BlockchainMiningEventArgs e)
        {
            Console.WriteLine("Saving block...");

            Task.Run(async () => {
                await _DatabaseDataService.CreateBlock(e.DocumentBlock);
            });

            _BlockchainDataService.BlockValidated_Outbound(e.DocumentBlock);
        }
        
        private void _NodePingTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            List<NodeRegistery> nodes_to_remove = new List<NodeRegistery>();

            foreach (var node in _Network.NodeList)
            {
                var still_there = _NodeDataService.NodePing(node.node_id);

                if (still_there == false)
                {
                    nodes_to_remove.Add(node);

                }
            }

            foreach (var node in nodes_to_remove)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine("Removing node " + node.ip_address.ToString());
                Console.ForegroundColor = ConsoleColor.White;
                _Network.NodeList.Remove(node);
            }
        }
    }
}
