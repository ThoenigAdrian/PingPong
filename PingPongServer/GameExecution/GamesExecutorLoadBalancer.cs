using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using NetworkLibrary.Utility;
using PingPongServer.GameExecution;
using PingPongServer.ServerGame;

namespace PingPongServer.GameExecution
{
    class GamesExecutorLoadBalancer
    {
        List<GamesExecutor> GamesExecutors = new List<GamesExecutor>();
        private LogWriterConsole Logger = new LogWriterConsole();
        private int PhyiscalCoreCount = 1;
        private int LogicalThreadCount = 1;

        public GamesExecutorLoadBalancer()
        {
            GetPhysicalCoreCount();
            GetLogicalCoreCount();
            for (int id = 0; id < PhyiscalCoreCount; id++)
            {
                GamesExecutors.Add(new GamesExecutor(id));
            }

        }

        private void AddGame(Game game)
        {
            GamesExecutor LeastStressedExecutor = GamesExecutors[0];
            foreach(GamesExecutor executor in GamesExecutors)
            {
                if (executor.GamesCount < LeastStressedExecutor.GamesCount)
                {
                    LeastStressedExecutor = executor;
                }
            }
            LeastStressedExecutor.AddGame(game);
        }

        private void GetLogicalCoreCount()
        {
            int coreCount = 0;
            LogicalThreadCount = Environment.ProcessorCount;
        }

        private void GetPhysicalCoreCount()
        {
            int coreCount = 0;
            foreach (ManagementBaseObject item in new ManagementObjectSearcher("Select NumberOfCores from Win32_Processor").Get())
            {
                coreCount += int.Parse(item["NumberOfCores"].ToString());
            }
            PhyiscalCoreCount = coreCount;
        }
    }

    
}
