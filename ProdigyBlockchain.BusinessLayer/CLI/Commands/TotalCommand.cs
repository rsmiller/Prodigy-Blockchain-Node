using System;

namespace Prodigy.BusinessLayer.CLI.Commands
{
    public class TotalCommand : ICommand
    {
        public string name { get; set; }
        public string command { get; set; }
        public string help { get; set; }

        public TotalCommand()
        {
            this.name = "Total Command";
            this.command = "total";
            this.help = "Returns blocks or transaction count";
        }

        public void Execute(ProdigyNode node, string console_text)
        {
            var results = console_text.Split(' ');

            if(results[1] == "blocks")
            {
                Console.WriteLine("Total blocks: " + node.Blockchain.Count());
            }
            else if (results[1] == "transactions")
            {
                Console.WriteLine("Total transactions: " + node.Blockchain.TransactionsCount());
            }
            else
            {
                Console.WriteLine("Possbile arguments are 'blocks' or 'transactions'");
            }
        }
    }
}
