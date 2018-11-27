using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stresstester.Testcases
{
    class CheckGameFlow
    {
        static void CreateDemoClient()
        {
            Thread RandomClientThread = new Thread(randomClient);
            RandomClientThread.Name = "Random Client";
            RandomClientThread.Start();
            Thread ProGamerClientThread = new Thread(proGamerClient);
            ProGamerClientThread.Name = "Pro Client";
            ProGamerClientThread.Start();
        }
    }
}
