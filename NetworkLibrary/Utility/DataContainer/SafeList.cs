using System;
using System.Collections.Generic;
using System.Threading;

namespace NetworkLibrary.Utility
{
    public class SafeList<T> : IDisposable
    {
        List<T> m_internalList;
        Semaphore m_listLock;

        public SafeList() : this (new List<T>())
        {
        }

        public SafeList(List<T> list)
        {
            m_listLock = new Semaphore(1, 1);
            m_internalList = list;
        }


        public List<T> Entries
        {
            get
            {
                m_listLock.WaitOne();
                try { return new List<T>(m_internalList); }
                finally { m_listLock.Release(); }
            }
        }

        public T this[int index]
        {
            get
            {
                m_listLock.WaitOne();
                try { return m_internalList[index]; }
                finally { m_listLock.Release(); }
            }
            set
            {
                m_listLock.WaitOne();
                try { m_internalList[index] = value; }
                finally { m_listLock.Release(); }
            }
        }

        public T GetEntry(int index)
        {
            m_listLock.WaitOne();
            try { return m_internalList[index]; }
            finally { m_listLock.Release(); }
        }

        public void SetEntry(int index, T entry)
        {
            m_listLock.WaitOne();
            try { m_internalList[index] = entry; }
            finally { m_listLock.Release(); }
        }

        public void Insert(int index, T entry)
        {
            m_listLock.WaitOne();
            try { m_internalList.Insert(index, entry); }
            finally { m_listLock.Release(); }
        }

        public void Add(T entry)
        {
            m_listLock.WaitOne();
            try { m_internalList.Add(entry); }
            finally { m_listLock.Release(); }
        }

        public bool Remove(T entry)
        {
            m_listLock.WaitOne();
            try { return m_internalList.Remove(entry); }
            finally { m_listLock.Release(); }
        }

        public void RemoveAt(int index)
        {
            m_listLock.WaitOne();
            try { m_internalList.RemoveAt(index); }
            finally { m_listLock.Release(); }
        }

        public void Clear()
        {
            m_listLock.WaitOne();
            try { m_internalList.Clear(); }
            finally { m_listLock.Release(); }
        }

        public int Count
        {
            get
            {
                m_listLock.WaitOne();
                try { return m_internalList.Count; }
                finally { m_listLock.Release(); }
            }
        }

        public void Dispose()
        {
            Clear();
            m_listLock.Dispose();
        }
    }
}
