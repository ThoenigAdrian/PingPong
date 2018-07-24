using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading;
using System.Timers;
using NetworkLibrary.Utility;
using PingPongServer.GameExecution;
using PingPongServer.ServerGame;

namespace PingPongServer.GameExecution
{
    class GamesExecutorLoadBalancer
    {
        List<GamesExecutor> GamesExecutors = new List<GamesExecutor>();
        List<SingleFireWaitCondition> WaitConditions = new List<SingleFireWaitCondition>();
        private LogWriterConsole Logger = new LogWriterConsole();
        private int PhyiscalCoreCount = 1;
        private int PhysicalThreadCount = 1;
        private int FrameRate = 240;
        private int index = 0;
        System.Timers.Timer FrameTimer;

        public GamesExecutorLoadBalancer()
        {
            GetPhysicalCoreCount();
            GetLogicalCoreCount();
            for (int id = 0; id < PhyiscalCoreCount; id++)
            {
                SingleFireWaitCondition FrameWaitCondition = new SingleFireWaitCondition(id);
                WaitConditions.Add(FrameWaitCondition);
                GamesExecutors.Add(new GamesExecutor(id, FrameWaitCondition));
                Thread ExecutorThread = new Thread(GamesExecutors[id].Run);
                ExecutorThread.Name = "Executor Thread " + id.ToString();
                ExecutorThread.Start();
            }
            float timerIntervall = 1000f / (FrameRate * PhyiscalCoreCount);
            FrameTimer = new System.Timers.Timer(timerIntervall);
            FrameTimer.Elapsed += OnNextFrameTimed;
            FrameTimer.AutoReset = true;
            FrameTimer.Enabled = true;

        }

        public void AddGame(Game game)
        {
            GamesExecutor LeastStressedExecutor = GamesExecutors[0];
            foreach (GamesExecutor executor in GamesExecutors)
            {
                if (executor.GamesCount < LeastStressedExecutor.GamesCount)
                {
                    LeastStressedExecutor = executor;
                }
            }
            game.StartGame(this);
            LeastStressedExecutor.AddGame(game);
        }

        private void OnNextFrameTimed(object source, ElapsedEventArgs e)
        {
            FrameTimer.Stop();
            if (index >= WaitConditions.Count)
            {
                index = 0;
            }
            WaitConditions[index].Fire();
            index++;
            FrameTimer.Start();
        }
               

        private void GetLogicalCoreCount()
        {
            PhysicalThreadCount = Environment.ProcessorCount;
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
