using Microsoft.Xna.Framework;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;
using System;

namespace PingPongClient.VisualizeLayer.Lobbies
{
    class PlayerEntry
    {
        DrawableString PlayerString { get; set; }

        public int Index { get; private set; }

        int m_team;
        public int Team
        {
            get { return m_team; }
            set
            {
                int previous = m_team;
                m_team = value;

                if (m_team < 0)
                    m_team = 0;

                if (m_team > 1)
                    m_team = 1;

                if (m_team != previous)
                    UpdatePlayerString();
            }
        }
        string TeamAsString { get { return Convert.ToString(Team + 1); } }

        public PlayerEntry(int index, DrawableString playerString)
        {
            PlayerString = playerString;
            playerString.Options.StringColor = Color.Yellow;

            Index = index;
            m_team = 0;

            UpdatePlayerString();
        }

        private void UpdatePlayerString()
        {
            PlayerString.Value = CreatePlayerString();
        }

        private string CreatePlayerString()
        {
            return "Player " + (Index + 1).ToString() + " - Team<" + TeamAsString + ">  Move with " + GetMovementKeyString() + ".";
        }

        private string GetMovementKeyString()
        {
            switch (Index)
            {
                case 0:
                    return "Up and Down";
                case 1:
                    return "W and S";
                case 2:
                    return "Num8 and Num2";
                default:
                    return "Invalid player index.";
            }
        }
    }
}
