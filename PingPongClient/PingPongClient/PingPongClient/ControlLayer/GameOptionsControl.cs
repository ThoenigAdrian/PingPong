using Microsoft.Xna.Framework;
using NetworkLibrary.DataPackages;
using PingPongClient.InputLayer.KeyboardInputs;
using PingPongClient.VisualizeLayer.Lobbies;
using PingPongClient.VisualizeLayer.Visualizers;

namespace PingPongClient.ControlLayer
{
    public class GameOptionsControl : SubControlInterface
    {
        GameOptions GameOptionLobby { get; set; }

        LobbyVisualizer LobbyVisualizer { get { return Visualizer as LobbyVisualizer; } }

        public override GameMode GetMode { get { return GameMode.Options; } }

        public GameOptionsControl(Control parent) : base(parent)
        {
            GameOptionLobby = new GameOptions();
            Visualizer = new LobbyVisualizer(GameOptionLobby);
        }

        public void SetServerIP(string serverIP)
        {
            GameOptionLobby.Status = "Connected to " + serverIP + "   Session: " + Network.ClientSession;
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
                    GameOptionLobby.Selection--;
                    break;

                case SelectionInputs.Down:
                    GameOptionLobby.Selection++;
                    break;

                default:
                    break;
            }
        }

        private void ApplyConfiguration()
        {
            ParentControl.RegistrationControl.MaxPlayers = GameOptionLobby.PlayerCount;
            ParentControl.RegistrationControl.RequestType = GameOptionLobby.SelectedOption;
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
                    GameOptionLobby.PlayerCount -= 2;
                    break;

                case SelectionInputs.Right:
                    GameOptionLobby.PlayerCount += 2;
                    break;

                default:
                    break;
            }
        }
    }
}
