﻿using Microsoft.Xna.Framework;
using PingPongClient.VisualizeLayer.Lobbies.SelectionLists;
using System;

namespace PingPongClient.VisualizeLayer.Lobbies
{
    class RequestLobby : LobbyInterface
    {
        public enum RequestOptions
        {
            Start,
            Join
        }

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
            set
            {
                int before = StartJoinSelection.Selection;
                StartJoinSelection.Selection = value;
                if (Selection != before)
                    SwitchMode();

                UpdatePlayerCount();
            }
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
        }

        public override Color GetBackgroundColor { get { return Color.Black; } }

        void SwitchMode()
        {
            if (SelectedOption == RequestOptions.Start)
                m_playerCount = 2;
                
            else
                m_playerCount = 1;    
        }

        void UpdatePlayerCount()
        {
            if (SelectedOption == RequestOptions.Start)
            {
                m_playerCount = Math.Max(2, m_playerCount);
                m_playerCount = Math.Min(6, m_playerCount);
                DrawablePlayerCount.Value = "Start a new game with a maximum of <" + m_playerCount + "> players.";
            }
            else
            {
                m_playerCount = Math.Max(1, m_playerCount);
                m_playerCount = Math.Min(3, m_playerCount);
                DrawablePlayerCount.Value = "Join a game with <" + m_playerCount + "> player/s on this computer.";
            }
        } 
    }
}
