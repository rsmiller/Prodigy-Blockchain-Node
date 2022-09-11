using System;


namespace Prodigy.BusinessLayer.CLI.Commands
{
    internal interface ICommand
    {
        string name { get; set; }
        string command { get; set; }
        string help { get; set; }
        void Execute(ProdigyNode node, string console_text);
    }
}
