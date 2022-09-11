using System;
using System.Collections.Generic;
using System.Net;

namespace Prodigy.BusinessLayer.Networks
{
    public class TestNetwork : Network
    {
        public TestNetwork(Guid node_id, string ticker, List<string> ip_seeds)
        {
            this.Name = "Prodigy Test";
            this.DefaultAPIPort = 8122;
            this.NetworkType = NetworkType.Testnet;
            this.CoinTicker = ticker;
            this.NodePassword = "4bb31eb6-d7a6-4327-a021-6c32055add07";
            this.Difficulty = 2;
            this.NodeId = node_id;
            this.NodePrivateKey = "PB123456789!!!$%";
            this.MiningConfirmationSize = 2;
            this.TransactionBlockHeight = 50;

            this.IPSeeds = new List<IPAddress>();
            
            foreach(var ip in ip_seeds)
            {
                try
                {
                    this.IPSeeds.Add(IPAddress.Parse(ip));
                }
                catch(Exception)
                {
                    throw new Exception("Could not convert " + ip + " to an internal IP address.");
                }
            };

        }
    }
}
