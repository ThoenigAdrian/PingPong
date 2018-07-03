using Microsoft.Xna.Framework;
using PingPongClient.VisualizeLayer.Lobbies.SelectionLists;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;
using System;

namespace PingPongClient.VisualizeLayer.Lobbies
{
    public enum RequestOptions
    {
        Matchmaking,
        Observe
    }

    class GameOptions : LobbyInterface
    {
        public RequestOptions SelectedOption { get { return (RequestOptions)Selection; } }

        public string Status
        {
            get { return DrawableStatus.Value; }
            set { DrawableStatus.Value = value; }
        }

        string Info { get; set; }

        int m_playerCount;
        public int PlayerCount
        {
            get { return m_playerCount; }
            set
            {
                m_playerCount = value;
                UpdatePlayerCount();
            }
        }

        public int Selection
        {
            get { return QueueingOptions.Selection; }
            set { QueueingOptions.Selection = value; }
        }

        DrawableString DrawableStatus;
        QueueSelection QueueingOptions;
        DrawableString DrawablePlayerCount;

        public GameOptions()
        {
            m_playerCount = 2;

            DrawableStatus = new DrawableString("", new Vector2(100, 100), Color.White);
            DrawablePlayerCount = new DrawableString("", new Vector2(100, 300), Color.White);
            QueueingOptions = new QueueSelection(new Vector2(100, 180));

            Strings.Add(DrawableStatus);
            Strings.Add(DrawablePlayerCount);
            Lists.Add(QueueingOptions);

            UpdatePlayerCount();

            QueueingOptions.SelectionChanged += UpdatePlayerCount;
        }

        public override Color GetBackgroundColor { get { return Color.Black; } }

        void UpdatePlayerCount()
        {
            m_playerCount = Math.Max(2, m_playerCount);
            m_playerCount = Math.Min(8, m_playerCount);

            switch (SelectedOption)
            {
                case RequestOptions.Matchmaking:
                    DrawablePlayerCount.Value = "Queue for matchmaking with a maximum of <" + m_playerCount + "> players.";
                    break;
                case RequestOptions.Observe:
                    DrawablePlayerCount.Value = "Watch a game with a maximum of <" + m_playerCount + "> players.";
                    break;
            }
        } 
    }
}
