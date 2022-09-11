using System;

namespace Prodigy.BusinessLayer.Wallet
{
    [Serializable]
    public class KeyPair
    {
        public string pub { get; set; }
        public string priv { get; set; }
    }
}
