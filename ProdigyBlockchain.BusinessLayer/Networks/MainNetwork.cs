using System;
using System.Collections.Generic;
using System.Net;

namespace Prodigy.BusinessLayer.Networks
{
    public class MainNetwork : Network
    {
        public MainNetwork(Guid node_id, string ticker, List<string> ip_seeds)
        {
            this.Name = "Prodigy Main";
            this.DefaultAPIPort = 7122;
            this.NetworkType = NetworkType.Mainnet;
            this.CoinTicker = ticker;
            this.NodePassword = "66e60c68-9ed3-43a3-a14c-505058a5bde4";
            this.Difficulty = 5;
            this.NodeId = node_id;
            this.NodePrivateKey = "PB123456789!!!$%";
            this.MiningConfirmationSize = 3;
            this.TransactionBlockHeight = 50;

            this.IPSeeds = new List<IPAddress>();

            foreach (var ip in ip_seeds)
            {
                try
                {
                    this.IPSeeds.Add(IPAddress.Parse(ip));
                }
                catch (Exception)
                {
                    throw new Exception("Could not convert " + ip + " to an internal IP address.");
                }
            };

        }
    }
}
