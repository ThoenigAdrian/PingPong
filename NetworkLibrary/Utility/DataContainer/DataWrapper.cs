using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetworkLibrary.Utility
{
    public class DataWrapper<T>
    {
        /// <summary>
        /// Return true if the data was already read at least once.
        /// </summary>
        public bool Read { get; private set; }

        T m_data;

        /// <summary>
        /// Get the wrapped data.
        /// </summary>
        public T Data
        {
            get
            {
                Read = true;
                return m_data;
            }
        }

        public DataWrapper(T data)
        {
            Read = false;
            m_data = data;
        }
    }
}
