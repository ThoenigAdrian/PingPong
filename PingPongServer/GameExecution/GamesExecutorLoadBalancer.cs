using System;
using System.Collections.Generic;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using NetworkLibrary.Utility;
using PingPongServer.ServerGame;
using XSLibrary.ThreadSafety.Locks;
using XSLibrary.Utility;

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
        private LogWriterConsole Logger = new LogWriterConsole();
        private uint PhyiscalCoreCount = 1;
        private int LogicalCoreCount = 1;
        private const uint FrameTime = 4;
        private const uint TimeInterval = 1;

        UnleashSignal[] WaitConditions = new UnleashSignal[TimeSlices];

        // need time slices because the minimum is still 1ms and we cant go lower than that
        // this means if we have more cores than slices two or more of the cores will start at the same time
        private const uint TimeSlices = FrameTime / TimeInterval;   
        GCHandle CallbackHandle;
        uint TimerID;
        int FireID = 0;

        public GamesExecutorLoadBalancer()
        {
            GetPhysicalCoreCount();
            GetLogicalCoreCount();

            for (int i = 0; i < TimeSlices; i++)
                WaitConditions[i] = new UnleashSignal();

            for (int id = 0; id < PhyiscalCoreCount; id++)
            {
                GamesExecutors.Add(new GamesExecutor(id, WaitConditions[id % (int)TimeSlices]));
                ThreadStarter.ThreadpoolDebug("Executor Thread " + id.ToString(), GamesExecutors[id].Run);
            }

            TimerCallbackMethod callback = new TimerCallbackMethod(OnNextFrameTimed);
            CallbackHandle = GCHandle.Alloc(callback, GCHandleType.Normal);

            uint userCtx = 0;
            TimerID = timeSetEvent(TimeInterval, 0, callback, ref userCtx, TIME_CALLBACK_FUNCTION | TIME_KILL_SYNCHRONOUS);
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
            if (FireID >= TimeSlices)
                FireID = 0;

            WaitConditions[FireID].Release();
        }
               

        private void GetLogicalCoreCount()
        {
            LogicalCoreCount = Environment.ProcessorCount;
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

            foreach (GamesExecutor game in GamesExecutors)
                game.Stop();

            foreach (UnleashSignal signal in WaitConditions)
                signal.Destroy();
        }
    }
}
