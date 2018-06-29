using Microsoft.Xna.Framework;
using NetworkLibrary.DataPackages;
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
        }

        private void HandleSelectInput()
        {
            SelectionInputs selection = Input.GetSelectionInput();

            switch (selection)
            {
                case SelectionInputs.Select:
                    ApplyConfiguration();
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

        private void ApplyConfiguration()
        {
            ParentControl.RegistrationControl.MaxPlayers = RequestLobby.PlayerCount;
            ParentControl.RegistrationControl.RequestType = RequestLobby.SelectedOption;
            ParentControl.RegistrationControl.ResetSelection();
            ParentControl.SwitchMode(GameMode.Registration);
        }

        protected override void ServerResponseActions(PackageInterface responsePackage)
        {
        }

        protected override void ResponseTimeoutActions(PackageType requestedPackageType)
        {
        }

        private void HandlePlayerCountInput()
        {
            SelectionInputs selection = Input.GetSelectionInput();

            switch (selection)
            {
                case SelectionInputs.Left:
                    RequestLobby.PlayerCount -= 2;
                    break;

                case SelectionInputs.Right:
                    RequestLobby.PlayerCount += 2;
                    break;

                default:
                    break;
            }
        }
    }
}
