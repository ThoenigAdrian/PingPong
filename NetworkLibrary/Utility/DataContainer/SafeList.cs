using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace NetworkLibrary.Utility.DataContainer
{
    class SafeList<T> : IDisposable
    {
        List<T> m_internalList;
        Semaphore m_listLock;

        public SafeList() : this(new List<T>())
        {
        }
        public SafeList(List<T> list)
        {
            m_listLock = new Semaphore(1,1);
            m_internalList = list;
        }

        public void Add(T entry)
        {
            m_listLock.WaitOne();
            try { m_internalList.Add(entry); }
            finally { m_listLock.Release(); }
        }

        public void Remove(T entry)
        {
            m_listLock.WaitOne();
            try { m_internalList.Remove(entry); }
            finally { m_listLock.Release(); }
        }

        public List<T> GetList()
        {
            m_listLock.WaitOne();
            try { return new List<T>(m_internalList); }
            finally { m_listLock.Release(); }
        }

        public void Dispose()
        {
            m_listLock.WaitOne();
            try { m_internalList.Clear(); }
            finally { m_listLock.Release(); }
            m_listLock.Dispose();
        }
    }
}
