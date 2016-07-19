﻿using System;
using Microsoft.Xna.Framework;
using NetworkLibrary.DataPackages;
using PingPongClient.VisualizeLayer.Visualizers;
using PingPongClient.VisualizeLayer.Lobbies;
using PingPongClient.InputLayer.KeyboardInputs;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using GameLogicLibrary.GameObjects;
using GameLogicLibrary;

namespace PingPongClient.ControlLayer
{
    public class PlayerRegistrationControl : SubControlInterface
    {
        public override GameMode GetMode { get { return GameMode.Registration; } }

        public RequestOptions RequestType { get; set; }
        public int MaxPlayers
        {
            get { return RegistrationLobby.MaxPlayers; }
            set { RegistrationLobby.MaxPlayers = value; }
        }
        
        PlayerRegistrationLobby RegistrationLobby { get; set; }

        public PlayerRegistrationControl(Control parent) : base(parent)
        {
            RegistrationLobby = new PlayerRegistrationLobby();
            RegistrationLobby.RegistrationFinishedEvent += OnReady;
            ResetSelection();
            Visualizer = new LobbyVisualizer(RegistrationLobby);
            RequestType = RequestOptions.Start;
        }

        public bool Hack()
        {
            ControlInputs hackInput = Input.GetControlInput();

            if (hackInput == ControlInputs.Pause)
            {
                Network.SendClientStart(2, new int[] { 0, 1 });

                Player[] players = new Player[2];
                players[0] = new Player(0, 0, GameInitializers.PLAYER_1_X);
                players[1] = new Player(1, 1, GameInitializers.PLAYER_2_X);

                ParentControl.GameControl.InitializeGame(players, new GameField(), new Ball());

                ParentControl.SwitchMode(GameMode.Game);

                return true;
            }

            return false;
        }

        public void ResetSelection()
        {
            RegistrationLobby.Selection = 0;
        }

        public override void HandleInput()
        {
            if (Hack())
                return;

            SelectionInputs selectionInput = Input.GetSelectionInput();

            if (selectionInput != SelectionInputs.NoInput)
            {
                switch (selectionInput)
                {
                    case SelectionInputs.Select:
                        RegistrationLobby.OnSelectionKey();
                        break;
                    case SelectionInputs.Delete:
                        RegistrationLobby.OnDeleteKey();
                        break;
                    case SelectionInputs.Up:
                        RegistrationLobby.Selection--;
                        break;
                    case SelectionInputs.Down:
                        RegistrationLobby.Selection++;
                        break;
                    case SelectionInputs.Left:
                        RegistrationLobby.OnLeft();
                        break;
                    case SelectionInputs.Right:
                        RegistrationLobby.OnRight();
                        break;
                }
            }
        }

        private void OnReady()
        {
            switch (RequestType)
            {
                case RequestOptions.Start:
                    Network.SendClientStart(MaxPlayers, RegistrationLobby.PlayerTeamWishes);
                    break;

                case RequestOptions.Join:
                    Network.SendClientJoin(MaxPlayers, RegistrationLobby.PlayerTeamWishes);
                    break;
            }

            IssueServerResponse(PackageType.ServerPlayerIDResponse);
            RegistrationLobby.SetStatus("Waiting for server response...");
        }

        public override void Update(GameTime gameTime)
        {
            RegistrationLobby.UpdateStatusVisibility();
        }

        protected override void ServerResponseActions(PackageInterface responsePackage)
        {
            ServerInitializeGameResponse response = responsePackage as ServerInitializeGameResponse;
            if (response == null)
                throw new Exception("Response reader failed to read package!");

            if(response.m_players == null || response.m_ball == null || response.m_field == null)
            {
                RegistrationLobby.SetStatus("Server response invalid!");
                return;
            }

            ParentControl.GameControl.InitializeGame(response.m_players, response.m_field, response.m_ball);

            ParentControl.SwitchMode(GameMode.Game);
        }

        protected override void ResponseTimeoutActions(PackageType requestedPackageType)
        {
            RegistrationLobby.SetStatus("Server response timeout!");
        }
    }
}