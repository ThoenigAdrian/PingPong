using System.Threading;

namespace NetworkLibrary.Utility
{
    public class DoubleBuffer<T> : DataContainer<T>
    {
        T m_buffer1;
        T m_buffer2;

        volatile bool m_switch;

        public DoubleBuffer()
        {
            Initialize();
        }

        private void Initialize()
        {
            m_switch = false;

            m_buffer1 = default(T);
            m_buffer2 = default(T);
        }

        public override T Read()
        {
            if(m_switch)
                return m_buffer1;
            else
                return m_buffer2;
        }

        public override void Write(T buffer)
        {
            if (!m_switch)
                m_buffer1 = buffer;
            else
                m_buffer2 = buffer;

            m_switch = !m_switch;
        }
    }
}
