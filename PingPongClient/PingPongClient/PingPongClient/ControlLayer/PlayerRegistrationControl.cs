using System;
using Microsoft.Xna.Framework;
using NetworkLibrary.DataPackages;
using PingPongClient.VisualizeLayer.Visualizers;
using PingPongClient.VisualizeLayer.Lobbies;
using PingPongClient.InputLayer.KeyboardInputs;

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
                    ParentControl.SwitchMode(GameMode.Game);
                    break;

                case RequestOptions.Join:
                    Network.SendClientJoin(MaxPlayers, RegistrationLobby.PlayerTeamWishes);
                    ParentControl.SwitchMode(GameMode.Game);
                    break;

                default:
                    return;
            }
        }

        public override void Update(GameTime gameTime)
        {
        }

        protected override void ServerResponseActions(PackageInterface responsePackage)
        {
            base.ServerResponseActions(responsePackage);
        }

        protected override void ResponseTimeoutActions(PackageType requestedPackageType)
        {
            base.ResponseTimeoutActions(requestedPackageType);
        }
    }
}
