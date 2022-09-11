using System;

namespace Prodigy.BusinessLayer.CLI.Commands
{
    public class ListCommand : ICommand
    {
        public string name { get; set; }
        public string command { get; set; }
        public string help { get; set; }

        public ListCommand()
        {
            this.name = "List Command";
            this.command = "list";
            this.help = "Returns a list of nodes of miners";
        }

        public void Execute(ProdigyNode node, string console_text)
        {
            var results = console_text.Split(' ');

            if(results[1] == "nodes")
            {
                if(node.Network.NodeList.Count > 0)
                {
                    Console.WriteLine("ID\tIP\tFirst Seen\tLast Ack");
                    foreach (var nd in node.Network.NodeList)
                    {
                        Console.WriteLine(nd.node_id + "\t" + nd.ip_address + "\t" + nd.first_seen + "\t" + nd.last_ack);
                    }
                }
                else
                {
                    Console.WriteLine("No nodes found.");
                }
            }
            else if (results[1] == "miners")
            {
                if(node.Network.MiningUsers.Count > 0)
                {
                    Console.WriteLine("External IP\tInternal IP\tWallet ID");
                    foreach (var user in node.Network.MiningUsers)
                    {
                        Console.WriteLine(user.external_ip_address + "\t" + user.internal_ip_address + "\t" + user.walled_id);
                    }
                }
                else
                {
                    Console.WriteLine("No miners found.");
                }
            }
            else
            {
                Console.WriteLine("Possbile arguments are 'nodes' or 'miners'");
            }
        }
    }
}
