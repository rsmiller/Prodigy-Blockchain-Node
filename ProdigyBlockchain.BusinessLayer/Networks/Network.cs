using Prodigy.BusinessLayer.Models;
using System;
using System.Net;
using System.Collections.Generic;

namespace Prodigy.BusinessLayer.Networks
{
    public interface INetwork
    {
        string Name { get; set; }
        NetworkType NetworkType { get; set; }
        int DefaultAPIPort { get; set; }
        string CoinTicker { get; set; }
        string NodePassword { get; set; }
        int Difficulty { get; set; }
        Guid NodeId { get; set; }
        string NodePrivateKey { get; set; }
        int TransactionBlockHeight { get; set; }
        int MiningConfirmationSize { get; set; }
        List<IPAddress> IPSeeds { get; set; }
        List<NodeRegistery> NodeList { get; set; }
        List<MiningUser> MiningUsers { get; set; }
    }

    public class Network : INetwork
    {
        public string Name { get; set; }
        public NetworkType NetworkType { get; set; }
        public int DefaultAPIPort { get; set; }
        public string CoinTicker { get; set; }
        public string NodePassword { get; set; }
        public int Difficulty { get; set; }
        public Guid NodeId { get; set; }
        public string NodePrivateKey { get; set; }
        public int TransactionBlockHeight { get; set; } = 50;
        public int MiningConfirmationSize { get; set; }
        public List<IPAddress> IPSeeds { get; set; } = new List<IPAddress>();
        public List<NodeRegistery> NodeList { get; set; } = new List<NodeRegistery>();
        public List<MiningUser> MiningUsers { get; set; } = new List<MiningUser>();
    }
}
