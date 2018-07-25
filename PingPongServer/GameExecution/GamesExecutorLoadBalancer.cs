using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        List<AutoResetEvent> WaitConditions = new List<AutoResetEvent>();
        private LogWriterConsole Logger = new LogWriterConsole();
        private int PhyiscalCoreCount = 1;
        private int PhysicalThreadCount = 1;
        private int FrameRate = 240;
        System.Timers.Timer FrameTimer;

        public GamesExecutorLoadBalancer()
        {
            GetPhysicalCoreCount();
            GetLogicalCoreCount();
            for (int id = 0; id < PhyiscalCoreCount; id++)
            {
                AutoResetEvent FrameWaitCondition = new AutoResetEvent(true);
                WaitConditions.Add(FrameWaitCondition);
                GamesExecutors.Add(new GamesExecutor(id, FrameWaitCondition));
                Thread ExecutorThread = new Thread(GamesExecutors[id].Run);
                ExecutorThread.Name = "Executor Thread " + id.ToString();
                ExecutorThread.Start();
            }
            double timerIntervall = 1000;
            timerIntervall = timerIntervall / (FrameRate * PhyiscalCoreCount);
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
            foreach(AutoResetEvent frameSignal in WaitConditions)
            {
                frameSignal.Set();
            }
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
