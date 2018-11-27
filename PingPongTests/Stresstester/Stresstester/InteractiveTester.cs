using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stresstester
{
    class InteractiveTester
    {
        static void openMultipleGamesInteractive(int numberOfGames)
        {
            do
            {
                Disconnect();
                openMultipleGames(numberOfGames);
                Logger.Log("Press enter to do it again - type \"exit\" to exit.");

            } while (Console.In.ReadLine() != "exit");
        }
    }
}
