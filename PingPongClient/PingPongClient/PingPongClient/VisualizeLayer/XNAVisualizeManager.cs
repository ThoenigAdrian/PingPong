using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PingPongClient.VisualizeLayer
{
    class XNAVisualizeManager
    {
        XNAInitializationData m_initData;
        public XNAInitializationData InitializeData
        {
            private get { return m_initData; }
            set
            {
                m_initData = value;
                SetResources();
            }
        }

        public XNAStructureVisualizer GameVisualizer { get; private set; }

        public XNAVisualizeManager()
        {
            GameVisualizer = new XNAStructureVisualizer();
        }

        public void SetResources()
        {
            if (GameVisualizer != null)
                (GameVisualizer as XNAVisualizer).Initialize(InitializeData);
        }
    }
}
