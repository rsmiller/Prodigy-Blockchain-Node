using System;
using System.Reflection;
using System.Threading.Tasks;

namespace Prodigy.BusinessLayer.CLI.Commands
{
    public class VersionCommand : ICommand
    {
        public string name { get; set; }
        public string command { get; set; }
        public string help { get; set; }

        public VersionCommand()
        {
            this.name = "Version Command";
            this.command = "version";
            this.help = "Displays the current nodes version";
        }

        public void Execute(ProdigyNode node, string console_text)
        {
            Console.WriteLine("Version: " + Assembly.GetExecutingAssembly().GetName().Version.ToString());
        }
    }
}
