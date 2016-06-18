using System;
using Microsoft.Xna.Framework;
using NetworkLibrary.DataPackages;
using PingPongClient.VisualizeLayer.Visualizers;
using PingPongClient.VisualizeLayer.Lobbies;
using PingPongClient.InputLayer.KeyboardInputs;
using NetworkLibrary.DataPackages.ServerSourcePackages;

namespace PingPongClient.ControlLayer
{
    public class PlayerRegistrationControl : SubControlInterface
    {
        public override GameMode GetMode { get { return GameMode.Registration; } }

        public RequestOptions RequestType { get; set; }
        public int MaxPlayers { get; set; }

        PlayerRegistrationLobby RegistrationLobby { get; set; }

        public PlayerRegistrationControl(Control parent) : base(parent)
        {
            RegistrationLobby = new PlayerRegistrationLobby();
            RegistrationLobby.RegistrationFinishedEvent += OnReady;
            Visualizer = new LobbyVisualizer(RegistrationLobby);
            RequestType = RequestOptions.Start;
            MaxPlayers = 2;
        }

        public override void HandleInput()
        {
            SelectionInputs selectionInput = Input.GetSelectionInput();

            if (selectionInput != SelectionInputs.NoInput)
            {
                switch (selectionInput)
                {
                    case SelectionInputs.Select:
                        RegistrationLobby.OnSelectionKey();
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
            ServerPlayerIDResponse response = responsePackage as ServerPlayerIDResponse;
            if (response == null)
                throw new Exception("Response reader failed to read package!");

            Input.ClearPlayerInput();

            int index = 0;
            foreach (int ID in response.m_playerIDs)
            {
                Input.AddPlayerInput(ID, index);
            }

            ParentControl.SwitchMode(GameMode.Game);
        }

        protected override void ResponseTimeoutActions(PackageType requestedPackageType)
        {
            RegistrationLobby.SetStatus("Server response timeout.");
            ParentControl.SwitchMode(GameMode.Game);
        }
    }
}
