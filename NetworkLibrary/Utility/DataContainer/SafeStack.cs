using System.Collections.Generic;
using System.Threading;

namespace NetworkLibrary.Utility
{
    public class SafeStack<T> : DataContainer<T>
    {
        Semaphore m_dataLock;

        List<T> m_data;

        public SafeStack()
        {
            m_data = new List<T>();

            m_dataLock = new Semaphore(1, 1);
        }

        public override T Read()
        {
            m_dataLock.WaitOne();

            if (m_data.Count <= 0)
            {
                m_dataLock.Release();
                return default(T);
            }

            T data = m_data[0];
            m_data.RemoveAt(0);

            m_dataLock.Release();

            return data;
        }

        public override void Write(T data)
        {
            m_dataLock.WaitOne();

            m_data.Add(data);

            m_dataLock.Release();
        }
    }
}
