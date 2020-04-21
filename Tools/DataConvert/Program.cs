using System;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;

namespace DataConvert
{
    class Program
    {
        static int Main(string[] args)
        {
            var rootCommand = new RootCommand
            {
                new Option<string>(
                    "--prop"
                    ,"EVE game props data (TypeIDs.yaml)"),
            };
            rootCommand.Description = "Convert EVE game's yaml files to sqlite3 database";

            rootCommand.Handler = CommandHandler.Create<string>(prop =>
            {
                bool flag = false;
                Console.WriteLine(prop);
                if (!string.IsNullOrEmpty(prop))
                {
                    flag = true;
                    if (!File.Exists(prop))
                    {
                        Console.WriteLine("File not found: " + prop);
                    }
                    else
                    {
                        PropYamlHandler.StartHandler(prop);
                    }
                }

                if (!flag)
                {
                    Console.WriteLine("Args invalid");
                }
            });

            return rootCommand.InvokeAsync(args).Result;
        }
    }
}
