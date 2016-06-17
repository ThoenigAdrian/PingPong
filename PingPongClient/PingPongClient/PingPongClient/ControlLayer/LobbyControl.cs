﻿using Microsoft.Xna.Framework;
using NetworkLibrary.DataPackages;
using NetworkLibrary.Utility;
using PingPongClient.InputLayer.KeyboardInputs;
using PingPongClient.VisualizeLayer.Lobbies;
using PingPongClient.VisualizeLayer.Visualizers;

namespace PingPongClient.ControlLayer
{
    public class LobbyControl : SubControlInterface
    {
        RequestLobby RequestLobby { get; set; }

        LobbyVisualizer LobbyVisualizer { get { return Visualizer as LobbyVisualizer; } }

        public override GameMode GetMode { get { return GameMode.Lobby; } }

        public LobbyControl(Control parent) : base(parent)
        {
            RequestLobby = new RequestLobby();

            Input.AddPlayerInput(0, 0);

            Visualizer = new LobbyVisualizer(RequestLobby);
        }

        public void SetServerIP(string serverIP)
        {
            RequestLobby.Status = "Connected to " + serverIP + "   Session: " + Network.ClientSession;
        }

        public override void HandleInput()
        {
            HandleSelectInput();
            HandlePlayerCountInput();
        }

        public override void Update(GameTime gameTime)
        {
            if (Network != null)
                Network.UpdateConnections();
        }


        private void HandleSelectInput()
        {
            SelectionInputs selection = Input.GetSelectionInput();

            switch (selection)
            {
                case SelectionInputs.Select:
                    SendRequestInput();
                    break;

                case SelectionInputs.Up:
                    RequestLobby.Selection--;
                    break;

                case SelectionInputs.Down:
                    RequestLobby.Selection++;
                    break;

                default:
                    break;
            }
        }

        private void SendRequestInput()
        {
            switch (RequestLobby.SelectedOption)
            {
                case RequestLobby.RequestOptions.Start:
                    Network.SendClientStart(RequestLobby.PlayerCount);
                    break;

                case RequestLobby.RequestOptions.Join:
                    Network.SendClientJoin(RequestLobby.PlayerCount);
                    break;

                default:
                    return;
            }

            IssueServerResponse(new ResponseRequest(PackageType.ServerData, 5000));
        }

        protected override void ServerResponseActions(PackageInterface responsePackage)
        {
            ParentControl.SwitchMode(GameMode.Game);
        }

        protected override void ResponseTimeoutActions(PackageType requestedPackageType)
        {
            ParentControl.NetworkDeathHandler(Network, 0);
        }

        private void HandlePlayerCountInput()
        {
            SelectionInputs selection = Input.GetSelectionInput();

            switch (selection)
            {
                case SelectionInputs.Left:
                    RequestLobby.PlayerCount--;
                    break;

                case SelectionInputs.Right:
                    RequestLobby.PlayerCount++;
                    break;

                default:
                    break;
            }
        }
    }
}
