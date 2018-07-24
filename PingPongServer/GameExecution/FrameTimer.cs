using System.Threading;

namespace PingPongServer.GameExecution
{
    class SingleFireWaitCondition
    {
        object FireInProgress = new object();
        Semaphore TimerSemaphore;
        public bool waiting;
        public SingleFireWaitCondition(int ID)
        {
            TimerSemaphore = new Semaphore(1, 1, "Single Fire Semaphore ID: " + ID.ToString());            
        }
        public void Fire()
        {
            lock(FireInProgress)
            {
                try
                {
                    TimerSemaphore.Release(1);
                }
                catch(SemaphoreFullException)
                {

                }
                    
            }            
        }
        public void Wait()
        {
            TimerSemaphore.WaitOne();
        }
    }
}
