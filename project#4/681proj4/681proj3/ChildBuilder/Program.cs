using System;
using System.Collections.Generic;
using Microsoft.Build.BuildEngine;
using Microsoft.Build.Framework;
using Microsoft.Build.Execution;
using wcfcom1;

namespace Buildserver1
{
    class ChildBuilder
    {
        static void BuildCsproj()
        {
            string projectFileName = @"..\..\..\files\Builder.csproj";

            ConsoleLogger logger = new ConsoleLogger();

            Dictionary<string, string> GlobalProperty = new Dictionary<string, string>();
            BuildRequestData BuildRequest = new BuildRequestData(projectFileName, GlobalProperty, null, new string[] { "Rebuild" }, null);
            BuildParameters bp = new BuildParameters();
            bp.Loggers = new List<ILogger> { logger };

            BuildResult buildResult = BuildManager.DefaultBuildManager.Build(bp, BuildRequest);

            Console.WriteLine();
        }
        static void ReceiveMessage()
        {
            Receiver receiver = new Receiver();
            receiver.CreateHost(new Uri("http://localhost:9077"));
            while (true)
            {
                CommMessage msg = receiver.GetMessage();
                Console.WriteLine("\nmessage is{0}", msg.Body);
                if (msg.Body == "quit")
                {
                    break;
                }
            }
        }

        static void Main(string[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkGreen;
            Console.BackgroundColor = ConsoleColor.White;
            Console.Write("\n  Building Buildserver1.csproj ");
            Console.Write("\n =========================\n\n");
            Console.ResetColor();
            try
            {
                BuildCsproj();
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.DarkRed;
                Console.BackgroundColor = ConsoleColor.White;
                Console.Write("\n\n  An error occured while trying to build the csproj file.\n  Details: {0}\n\n", ex.Message);
                Console.ResetColor();
            }
            ReceiveMessage();
           // Console.ReadLine();
            
        }
    }

}

