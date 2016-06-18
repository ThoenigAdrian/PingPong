using System;

namespace NetworkLibrary.Utility
{
    public class OneShotTimer
    {
        DateTime m_start;
        bool m_timerOverflow;
        long m_timerEnd;

        public OneShotTimer(long microSeconds, bool start = true)
        {
            m_timerEnd = microSeconds;

            if (start)
                Restart();
            else
                m_timerOverflow = true;
        }

        public void TimerInterval(long microSeconds)
        {
            m_timerEnd = microSeconds;
            m_timerOverflow = false;
        }

        public void Restart()
        {
            m_start = DateTime.Now;
            m_timerOverflow = false;
        }

        private bool TimerOverflow()
        {
            if(m_timerOverflow)
                return true;

            DateTime now = DateTime.Now;

            if(m_start.Ticks + (m_timerEnd * 10) < now.Ticks)
            {
                m_timerOverflow = true;
            }

            return m_timerOverflow;
        }

        public static bool operator ==(OneShotTimer timer, bool val)
        {
            return timer.TimerOverflow() == val;
        }

        public static bool operator !=(OneShotTimer timer, bool val)
        {
            return timer != val;
        }
        
        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }
        public override bool Equals(object o)
        {
            throw new NotImplementedException();
        }
    }
}
