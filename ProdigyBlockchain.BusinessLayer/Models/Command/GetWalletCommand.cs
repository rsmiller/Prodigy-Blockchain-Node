
namespace Prodigy.BusinessLayer.Models.Command
{
    public class GetWalletCommand
    {
        public string wallet_address { get; set; }
        public string username { get; set; }
        public string passphrase { get; set; }
    }
}
