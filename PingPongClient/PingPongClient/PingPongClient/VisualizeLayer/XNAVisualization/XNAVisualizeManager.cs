namespace PingPongClient.VisualizeLayer.XNAVisualization
{
    class XNAVisualizeManager : VisualizeManager
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

        public XNAVisualizeManager()
        {
            LobbyVisualizer = new XNALobbyVisualizer();
            GameVisualizer = new XNAStructureVisualizer();
        }

        public void SetResources()
        {
            if (GameVisualizer != null)
                (GameVisualizer as XNAVisualizer).Initialize(InitializeData);

            if (LobbyVisualizer != null)
                (LobbyVisualizer as XNAVisualizer).Initialize(InitializeData);
        }
    }
}
