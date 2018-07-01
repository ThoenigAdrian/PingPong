using Microsoft.Xna.Framework;
using NetworkLibrary.Utility;
using PingPongClient.VisualizeLayer.Lobbies.SelectionLists;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;
using System;
using System.Collections.Generic;
using XSLibrary.Utility;

namespace PingPongClient.VisualizeLayer.Lobbies
{
    class PlayerRegistrationLobby : LobbyInterface
    {
        public delegate void RegistrationFinishedHandler();
        public event RegistrationFinishedHandler RegistrationFinishedEvent;

        RegistrationOptions RegistrationSelection { get; set; }

        List<PlayerEntry> RegisteredPlayers { get; set; }
        public int RegisteredPlayersCount { get { return RegisteredPlayers.Count; } }

        public override Color GetBackgroundColor { get { return Color.Black; } }

        SelectionEntry StringAddPlayerEntry { get; set; }

        DrawableString Status { get; set; }
        OneShotTimer StatusTimer { get; set; }

        int m_maxPlayers;
        public int MaxPlayers
        {
            get { return m_maxPlayers; }
            set
            {
                m_maxPlayers = value;

                if (MaxLocalPlayers > RegisteredPlayersCount)
                {
                    InsertAddPlayerString();
                }
                else
                {
                    while (RegisteredPlayersCount > MaxLocalPlayers)
                        OnDeleteKey();
                }

                UpdateAfterAddPlayer();
                UpdateEntryPositions();
            }
        }

        int MaxLocalPlayers { get { return Math.Min(m_maxPlayers, 3); } }

        public int Selection
        {
            get { return RegistrationSelection.Selection; }
            set { RegistrationSelection.Selection = value; }
        }

        public int[] PlayerTeamWishes
        {
            get
            {
                int[] wishes = new int[RegisteredPlayersCount];
                for(int i = 0; i < RegisteredPlayersCount; i++)
                {
                    wishes[i] = RegisteredPlayers[i].Team;
                }

                return wishes;
            }
        }

        public PlayerRegistrationLobby()
        {
            StatusTimer = new OneShotTimer(5 * 1000 * 1000, false);

            m_maxPlayers = 2;

            Status = CreateStatusDrawString();
            Status.Visible = false;
            Strings.Add(Status);
            Strings.Add(new DrawableString("Press <Delete> to remove a player.", new Vector2 (117, 50), Color.White));

            RegistrationSelection = new RegistrationOptions();
            RegisteredPlayers = new List<PlayerEntry>();

            RegistrationSelection.TopLeft = new Vector2(100, 130);
            StringAddPlayerEntry = CreateAddPlayerStringEntry();
            InsertAddPlayerString();

            OnAddPlayerSelection();

            UpdateEntryPositions();

            Lists.Add(RegistrationSelection);
        }

        public void SetStatus(string status)
        {
            Status.Value = status;
            Status.Visible = true;
            StatusTimer.Restart();
        }

        public void UpdateStatusVisibility()
        {
            Status.Visible = StatusTimer == false;
        }

        public void OnSelectionKey()
        {
            if (SelectionIsReady() && RegistrationFinishedEvent != null)
            {
                RegistrationFinishedEvent.Invoke();
            }

            else if(SelectionIsAddPlayer())
            {
                OnAddPlayerSelection();
            }
        }

        public void OnDeleteKey()
        {
            if (RegisteredPlayersCount <= 1)
                return;

            int removedIndex = RegisteredPlayersCount - 1;

            RegisteredPlayers.RemoveAt(removedIndex);
            RegistrationSelection.ListEntries.RemoveAt(removedIndex);

            if (RegisteredPlayersCount == MaxLocalPlayers - 1)
                InsertAddPlayerString();
            else if (Selection > 0 && !(Selection < removedIndex))
                Selection--;

            UpdateEntryPositions();
        }

        public void OnLeft()
        {
            if (!SelectionIsPlayer())
                return;

            RegisteredPlayers[RegistrationSelection.Selection].Team -= 1;
        }

        public void OnRight()
        {
            if (!SelectionIsPlayer())
                return;

            RegisteredPlayers[RegistrationSelection.Selection].Team += 1;
        }

        void OnAddPlayerSelection()
        {
            if (RegisteredPlayersCount >= MaxLocalPlayers)
                return;

            DrawableString playerString = new DrawableString("", new Vector2(0, 0), Color.White);
            PlayerEntry entry = new PlayerEntry(RegisteredPlayersCount, playerString);
            RegistrationSelection.ListEntries.Insert(RegisteredPlayersCount, new SelectionEntry(playerString, new Selector()));
            RegisteredPlayers.Add(entry);

            UpdateAfterAddPlayer();
            UpdateEntryPositions();
        }

        void InsertAddPlayerString()
        {
            if(!RegistrationSelection.ListEntries.Contains(StringAddPlayerEntry))
                RegistrationSelection.ListEntries.Insert(RegisteredPlayersCount, StringAddPlayerEntry);
        }

        void UpdateAfterAddPlayer()
        {
            if (RegisteredPlayersCount >= MaxLocalPlayers)
            {
                RegistrationSelection.ListEntries.Remove(StringAddPlayerEntry);
            }
            else
            {
                Selection++;
            }
        }

        void UpdateEntryPositions()
        {
            int index = 0;
            foreach (SelectionEntry entry in RegistrationSelection.ListEntries)
            {
                entry.Position = new Vector2(0, index * 30 + (index == RegistrationSelection.ListEntries.Count - 1 ? 50 : 0));
                index++;
            }
        }

        DrawableString CreateStatusDrawString()
        {
            return new DrawableString("", new Vector2(100, 350), Color.White);
        }

        SelectionEntry CreateAddPlayerStringEntry()
        {
            return new SelectionEntry(new DrawableString("<Enter> Add local Player", new Vector2(0, 30), Color.White), new Selector());
        }

        bool SelectionIsPlayer()
        {
            return RegistrationSelection.Selection < RegisteredPlayersCount;
        }

        bool SelectionIsAddPlayer()
        {
            return RegistrationSelection.Selection == RegisteredPlayersCount;
        }

        bool SelectionIsReady()
        {
            return RegistrationSelection.Selection == RegistrationSelection.ListEntries.Count - 1;
        }
    }

}