using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using NetworkLibrary.Utility;
using PingPongServer.ServerGame;

namespace PingPongServer.GameExecution
{
    class GamesExecutorLoadBalancer : IDisposable
    {
        const uint TIME_CALLBACK_FUNCTION = 0x001;
        const uint TIME_KILL_SYNCHRONOUS = 0x100;

        public delegate void TimerCallbackMethod(uint id, uint msg, ref uint userCtx, uint rsv1, uint rsv2);

        [DllImport("winmm.dll")]
        public static extern uint timeSetEvent(uint delay, uint resolution, TimerCallbackMethod callback, ref uint userCtx, uint fuEvent);
        [DllImport("winmm.dll")]
        public static extern uint timeKillEvent(uint timerID);

        List<GamesExecutor> GamesExecutors = new List<GamesExecutor>();
        List<AutoResetEvent> WaitConditions = new List<AutoResetEvent>();
        private LogWriterConsole Logger = new LogWriterConsole();
        private uint PhyiscalCoreCount = 1;
        private int PhysicalThreadCount = 1;
        private uint FrameRate = 240;
        GCHandle CallbackHandle;
        uint TimerID;
        int FireID = 0;

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
            uint timerIntervall = 1000;
            timerIntervall = timerIntervall / (FrameRate * PhyiscalCoreCount);
            if (timerIntervall == 0)
                timerIntervall = 1;

            TimerCallbackMethod callback = new TimerCallbackMethod(OnNextFrameTimed);
            CallbackHandle = GCHandle.Alloc(callback);

            uint userCtx = 0;
            TimerID = timeSetEvent(timerIntervall, 0, callback, ref userCtx, TIME_CALLBACK_FUNCTION | TIME_KILL_SYNCHRONOUS);
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

        private void OnNextFrameTimed(uint id, uint msg, ref uint userCtx, uint rsv1, uint rsv2)
        {
            FireID++;
            if (FireID >= PhyiscalCoreCount)
                FireID = 0;

            WaitConditions[FireID].Set();
        }
               

        private void GetLogicalCoreCount()
        {
            PhysicalThreadCount = Environment.ProcessorCount;
        }

        private void GetPhysicalCoreCount()
        {
            PhyiscalCoreCount = 0;
            foreach (ManagementBaseObject item in new ManagementObjectSearcher("Select NumberOfCores from Win32_Processor").Get())
            {
                PhyiscalCoreCount += uint.Parse(item["NumberOfCores"].ToString());
            }
        }

        public void Dispose()
        {
            timeKillEvent(TimerID);
            CallbackHandle.Free();
        }
    }
}
