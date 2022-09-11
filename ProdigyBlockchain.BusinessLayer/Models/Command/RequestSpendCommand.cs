
namespace Prodigy.BusinessLayer.Models.Command
{
    public class RequestSpendCommand
    {
        public string wallet_id { get; set; }
        public decimal amount { get; set; }
        public string note { get; set; }
    }
}
