using Microsoft.Xna.Framework;
using PingPongClient.VisualizeLayer.Lobbies.SelectionLists;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;
using System.Collections.Generic;

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

        public int Selection
        {
            get { return RegistrationSelection.Selection; }
            set { RegistrationSelection.Selection = value; }
        }

        public PlayerRegistrationLobby()
        {
            RegistrationSelection = new RegistrationOptions();
            RegisteredPlayers = new List<PlayerEntry>();

            StringAddPlayerEntry = CreateAddPlayerStringEntry();
            RegistrationSelection.ListEntries.Insert(0, StringAddPlayerEntry);
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
            if (RegisteredPlayersCount > 2)
                return;

            DrawableString playerString = new DrawableString("", new Vector2(0, 0), Color.White);
            PlayerEntry entry = new PlayerEntry(RegisteredPlayersCount, playerString);
            RegisteredPlayers.Add(entry);
            RegistrationSelection.ListEntries.Add(new SelectionEntry(playerString, new Selector()));

            UpdateAfterAddPlayer();
            UpdateEntryPositions();
        }

        void UpdateAfterAddPlayer()
        {
            if(RegisteredPlayersCount > 2)
            {
                RegistrationSelection.ListEntries.Remove(StringAddPlayerEntry);
            }
        }

        void UpdateEntryPositions()
        {
            int index = 0;
            foreach (SelectionEntry entry in RegistrationSelection.ListEntries)
            {
                entry.Position = new Vector2(0, index * 30 + (index > RegisteredPlayersCount ? 50 : 0));
                index++;
            }
        }

        SelectionEntry CreateAddPlayerStringEntry()
        {
            return new SelectionEntry(new DrawableString("Add local Player", new Vector2(0, 30), Color.White), new Selector());
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
            return RegistrationSelection.Selection == RegisteredPlayersCount + 1;
        }
    }

}