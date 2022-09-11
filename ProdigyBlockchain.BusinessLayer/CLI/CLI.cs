using Prodigy.BusinessLayer.CLI.Commands;
using System;
using System.Collections.Generic;

namespace Prodigy.BusinessLayer.CLI
{
    public class CLI
    {
        private List<ICommand> _Commands { get; set; } = new List<ICommand>();
        private ProdigyNode _Node { get; set; }
        private bool _BreakLoop = false;
        public CLI(ProdigyNode documentNode)
        {
            _Node = documentNode;

            // Add commands
            AddCommands();

            // Start the console/terminal loop
            CLILoop();
        }

        private void AddCommands()
        {
            _Commands.Add(new TotalCommand());
            _Commands.Add(new ListCommand());
            _Commands.Add(new SetCommand());
            _Commands.Add(new DoCommand());
        }

        public void CLILoop()
        {
            Console.WriteLine("Node ready.");
            // kill to break
            while (_BreakLoop == false)
            {
                ProcessInput(Console.ReadLine());
            }
        }

        private void ProcessInput(string console_input)
        {
            if (console_input.Length > 50) // buffer was filled with info
                return;

            if(console_input == "") // happens
                return;

            // Do help
            if (console_input == "help")
            {
                foreach (var cmd in _Commands)
                {
                    Console.WriteLine(cmd.command + "\t" + cmd.help);
                }
            }

            if (console_input == "kill" || console_input == "exit") // break the looop
            {
                _BreakLoop = true;
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Have a nice day!");
                Console.ForegroundColor = ConsoleColor.White;
            }
            else
            {
                // Scan commands and execute
                foreach(var cmd in _Commands)
                {
                    if(console_input.Contains(cmd.command))
                    {
                        cmd.Execute(_Node, console_input);
                    }
                }
            }
        }
    }
}
