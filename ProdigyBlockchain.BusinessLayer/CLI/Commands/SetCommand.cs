using System;
using System.Linq;

namespace Prodigy.BusinessLayer.CLI.Commands
{
    public class SetCommand : ICommand
    {
        public string name { get; set; }
        public string command { get; set; }
        public string help { get; set; }

        public SetCommand()
        {
            this.name = "Set Command";
            this.command = "set";
            this.help = "Sets variables or performs functions";
        }

        public void Execute(ProdigyNode node, string console_text)
        {
            var results = console_text.Split(' ');

            if(results[1] == "genesis")
            {
                Guid block_id = Guid.Empty;

                if (Guid.TryParse(results[2], out block_id))
                {
                    var block = node.Blockchain.Where(m => m.BlockId == block_id).FirstOrDefault();
                    if (block != null)
                    {
                        var index = node.Blockchain.IndexOf(block);

                        Console.WriteLine("Continueing will remove " + (index - 1) + " from the blockchain. Continue? y/n");
                        var input = Console.ReadLine();
                        if(input == "y")
                        {
                            node.SetGenesisBlock(block_id);
                        }
                    }
                    else
                    {
                        Console.WriteLine("No block found.");
                    }
                }
                else
                {
                    Console.WriteLine(results[2] + " if not a uuid/guid.");
                }
                
            }
            else
            {
                Console.WriteLine("Possbile arguments are 'genesis'");
            }
        }
    }
}
