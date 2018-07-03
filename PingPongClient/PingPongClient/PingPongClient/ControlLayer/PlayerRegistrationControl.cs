using Microsoft.Xna.Framework;
using PingPongClient.VisualizeLayer.Visualizers;
using PingPongClient.VisualizeLayer.Lobbies;
using PingPongClient.InputLayer.KeyboardInputs;

namespace PingPongClient.ControlLayer
{
    public class PlayerRegistrationControl : SubControlInterface
    {
        public override GameMode GetMode { get { return GameMode.Registration; } }

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
        }

        public void ResetSelection()
        {
            RegistrationLobby.Selection = 0;
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
            Network.SendClientQueueMatchmaking(MaxPlayers, RegistrationLobby.PlayerTeamWishes);
            ParentControl.SwitchMode(GameMode.Status);
        }

        public override void Update(GameTime gameTime)
        {
            RegistrationLobby.UpdateStatusVisibility();
        }
    }
}
