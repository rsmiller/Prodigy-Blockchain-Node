using System;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.CLI.Commands
{
    public class DoCommand : ICommand
    {
        public string name { get; set; }
        public string command { get; set; }
        public string help { get; set; }

        public DoCommand()
        {
            this.name = "Do Command";
            this.command = "do";
            this.help = "Performs an audit of the blockchain which validates the data for changes";
        }

        public void Execute(ProdigyNode node, string console_text)
        {
            var results = console_text.Split(' ');

            if(results[1] == "audit")
            {
                Console.WriteLine("Validating every block. This may take awhile to complete.");
                Task.Run(async () => {
                    var result = await node.PerformAudit();

                    if(result == true)
                    {
                        Console.ForegroundColor = ConsoleColor.Green;
                        Console.WriteLine("Audit completed succesfully.");
                        Console.ForegroundColor = ConsoleColor.White;
                    }
                    else
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine("Audit has failed");
                        Console.ForegroundColor = ConsoleColor.White;
                    }

                });
            }
            else
            {
                Console.WriteLine("Possbile arguments are 'audit'");
            }
        }
    }
}
