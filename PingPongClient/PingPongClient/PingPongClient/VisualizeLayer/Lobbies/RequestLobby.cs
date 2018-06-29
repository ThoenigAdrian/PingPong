using Microsoft.Xna.Framework;
using PingPongClient.VisualizeLayer.Lobbies.SelectionLists;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;
using System;

namespace PingPongClient.VisualizeLayer.Lobbies
{
    public enum RequestOptions
    {
        Matchmaking,
        Start,
        Join
    }

    class RequestLobby : LobbyInterface
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
            get { return StartJoinSelection.Selection; }
            set { StartJoinSelection.Selection = value; }
        }

        DrawableString DrawableStatus;
        StartJoinSelection StartJoinSelection;
        DrawableString DrawablePlayerCount;

        public RequestLobby()
        {
            m_playerCount = 2;

            DrawableStatus = new DrawableString("", new Vector2(100, 100), Color.White);
            DrawablePlayerCount = new DrawableString("", new Vector2(100, 300), Color.White);
            StartJoinSelection = new StartJoinSelection(new Vector2(100, 180));

            Strings.Add(DrawableStatus);
            Strings.Add(DrawablePlayerCount);
            Lists.Add(StartJoinSelection);

            UpdatePlayerCount();

            StartJoinSelection.SelectionChanged += UpdatePlayerCount;
        }

        public override Color GetBackgroundColor { get { return Color.Black; } }

        void UpdatePlayerCount()
        {
            m_playerCount = Math.Max(2, m_playerCount);
            m_playerCount = Math.Min(6, m_playerCount);

            switch (SelectedOption)
            {
                case RequestOptions.Start:
                    DrawablePlayerCount.Value = "Start a new game with a maximum of <" + m_playerCount + "> players.";
                    break;
                case RequestOptions.Join:
                    DrawablePlayerCount.Value = "Join a game with a maximum of <" + m_playerCount + "> players.";
                    break;
                case RequestOptions.Matchmaking:
                    DrawablePlayerCount.Value = "Queue for matchmaking with a maximum of <" + m_playerCount + "> players.";
                    break;
            }
        } 
    }
}
