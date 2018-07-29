using System;
using System.Management;
using System.Runtime.InteropServices;
using NetworkLibrary.NetworkImplementations.ConnectionImplementations;
using NetworkLibrary.Utility;
using PingPongServer.ServerGame;
using XSLibrary.ThreadSafety.Containers;
using XSLibrary.ThreadSafety.Locks;
using XSLibrary.Utility;

namespace PingPongServer.GameExecution
{
    /// <summary>
    /// 
    /// General:
    /// The purpose of this Class is to create multiple Threads which execute the Games. 
    /// The responsibility of this class is to Load Balance each GamesExecutor to have around the same number of games and therefore around the same load (see AddGame Method)
    /// 
    /// Threading:
    /// The number of Threads which will be started is equal to the number of LogicalCores (also called Threads/Hyperthreads).
    /// Creating more threads would decrease performance, one previous approach was to have a Thread for each game which resulted in bad performance due to a lot of context switching.
    /// 
    /// CPU Load Balancing:
    /// Also the threads won't be started all at once but instead will be evened out over time
    /// 
    /// ^ Thread Nr.
    /// | |Thread 1|----------------------------remaining time
    /// | |Thread 2|----------------------------remaining time
    /// | |Thread 3|----------------------------remaining time
    /// | |Thread 4|----------------------------remaining time
    /// -------------------------------------------------> t/s
    /// 
    /// VS.
    /// 
    /// ^ Thread Nr.
    /// | |Thread 1|-----------------------------remaining time
    /// | ----------|Thread 2|-------------------remaining time
    /// | --------------------|Thread 3|---------remaining time
    /// | ------------------------------|Thread 4|emaining time
    /// -------------------------------------------------> t/s
    /// 
    /// The period between awakening a sepcific GameExecutor is dependent on the frame rate.
    /// So for a frame rate of 250 FPS => a 4 millisecond period is needed. 
    /// The minimum time period on windows is 1 milli second.
    /// This means if we have more logical cores than FramePeriod/MinimumTimerPeriod we need to start mutliple threads at once.
    /// We'll call this CPULoadBalanceFactor . The CPU Balance Factor can be increased by decreasing the frame rate.
    /// </summary>
    class GamesExecutorLoadBalancer : IDisposable
    {
        const uint TIME_CALLBACK_FUNCTION = 0x001;
        const uint TIME_KILL_SYNCHRONOUS = 0x100;

        public delegate void TimerCallbackMethod(uint id, uint msg, ref uint userCtx, uint rsv1, uint rsv2);

        [DllImport("winmm.dll")]
        public static extern uint timeSetEvent(uint delay, uint resolution, TimerCallbackMethod callback, ref uint userCtx, uint fuEvent);
        [DllImport("winmm.dll")]
        public static extern uint timeKillEvent(uint timerID);

        SafeList<GamesExecutor> GamesExecutors = new SafeList<GamesExecutor>();
        private LogWriterConsole Logger = new LogWriterConsole();
        private uint PhyiscalCoreCount = 1;
        private int LogicalCoreCount = 1;
        private const uint FrameTimeMilliseconds = 4; // Every 4 milliseconds => Framerate = 250 FPS
        private const uint TimerIntervalMilliseconds = 1;

        // If we have more logical cores than FramePeriod/MinimumTimerPeriod we need to start mutliple threads at once.
        private const uint CPULoadBalanceFactor = FrameTimeMilliseconds / TimerIntervalMilliseconds;

        public int PlayersCurrentlyInGames()
        {
            int playersCurrentlyInGame = 0;
            foreach(GamesExecutor executor in GamesExecutors.Entries)
            {
                playersCurrentlyInGame += executor.PlayersCurrentlyInGames();
            }
            return playersCurrentlyInGame;
        }

        UnleashSignal[] FrameWaitConditions = new UnleashSignal[CPULoadBalanceFactor];
        
        GCHandle CallbackHandle;
        uint TimerID;
        int FireID = 0;

        public GamesExecutorLoadBalancer()
        {
            Logger.LoadBalancerLog("Initialising...");
            GetPhysicalCoreCount();
            GetLogicalCoreCount();
            CreateFrameWaitConditions();
            StartGamesExecutionThreads();
            InitializeFrameTimer();
        }

        public void AddGame(Game game)
        {
            GamesExecutor LeastStressedExecutor = GamesExecutors[0];
            foreach (GamesExecutor executor in GamesExecutors.Entries)
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
            if (FireID >= CPULoadBalanceFactor)
                FireID = 0;
            FrameWaitConditions[FireID].Release();
            FireID++;
        }

        private void InitializeFrameTimer()
        {
            TimerCallbackMethod callback = new TimerCallbackMethod(OnNextFrameTimed);
            CallbackHandle = GCHandle.Alloc(callback, GCHandleType.Normal);
            uint userCtx = 0;
            TimerID = timeSetEvent(TimerIntervalMilliseconds, 0, callback, ref userCtx, TIME_CALLBACK_FUNCTION | TIME_KILL_SYNCHRONOUS);
        }

        public bool RejoinClientToGame(NetworkConnection clientWantsRejoin)
        {
            foreach(GamesExecutor executor in GamesExecutors.Entries)
            {
                if(executor.RejoinClientToGame(clientWantsRejoin))
                    return true;
            }
            return false;
        }

        private void StartGamesExecutionThreads()
        {
            Logger.LoadBalancerLog("Starting Games Executors according to Threadcount of CPU");
            for (int id = 0; id < LogicalCoreCount; id++)
            {
                GamesExecutors.Add(new GamesExecutor(id, FrameWaitConditions[id % (int)CPULoadBalanceFactor]));
                ThreadStarter.ThreadpoolDebug("Games Executor Thread " + id.ToString(), GamesExecutors[id].Run);
            }
            
        }

        private void CreateFrameWaitConditions()
        {
            for (int i = 0; i < CPULoadBalanceFactor; i++)
                FrameWaitConditions[i] = new UnleashSignal();
        }
               
        private void GetLogicalCoreCount()
        {
            LogicalCoreCount = Environment.ProcessorCount;

            // There must be at least one thread otherwise this program shouldn't be able to run. This is just a safe guard in case something is wrong with the lines above.
            if (LogicalCoreCount == 0)
            {
                LogicalCoreCount = 1;
            }
            Logger.LoadBalancerLog("System has " + LogicalCoreCount.ToString() + " Threads");
        }

        private void GetPhysicalCoreCount()
        {
            PhyiscalCoreCount = 0;
            foreach (ManagementBaseObject item in new ManagementObjectSearcher("Select NumberOfCores from Win32_Processor").Get())
            {
                PhyiscalCoreCount += uint.Parse(item["NumberOfCores"].ToString());
            }

            // There must be at least one core otherwise this program shouldn't be able to run. This is just a safe guard in case something is wrong with the lines above.
            if (PhyiscalCoreCount == 0)
            {
                PhyiscalCoreCount = 1;
            }

            Logger.LoadBalancerLog("System has " + PhyiscalCoreCount.ToString() + " Physical Cores");
                
        }

        public bool AddObserver(NetworkConnection observerConnection)
        {
            bool success = false;
            foreach (GamesExecutor executor in GamesExecutors.Entries)
            {
                if (executor.AddObserversToGame(observerConnection))
                {
                    success = true;
                    break;
                }
            }
            return success;
        }

        public void Dispose()
        {
            Logger.LoadBalancerLog("Dispose is being called and therefore being cleaned up nicely");
            timeKillEvent(TimerID);

            foreach (GamesExecutor gamesExecutor in GamesExecutors.Entries)
            {
                gamesExecutor.Stop();
                GamesExecutors.Remove(gamesExecutor);
            }
                

            foreach (UnleashSignal signal in FrameWaitConditions)
                signal.Destroy();

        }
    }
}
