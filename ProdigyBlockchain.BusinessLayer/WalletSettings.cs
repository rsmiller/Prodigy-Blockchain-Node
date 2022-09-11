
namespace Prodigy.BusinessLayer
{
    public interface IWalletSettings
    {
        string Prefix { get; set; }
        string PrivateKey { get; set; }
        string WalletDirectory { get; set; }
    }

    public class WalletSettings : IWalletSettings
    {
        public string Prefix { get; set; }
        public string PrivateKey { get; set; }
        public string WalletDirectory { get; set; }
    }
}
