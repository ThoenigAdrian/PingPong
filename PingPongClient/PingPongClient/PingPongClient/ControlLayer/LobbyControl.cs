using Microsoft.Xna.Framework;
using PingPongClient.InputLayer;
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
            RequestLobby.Status = "Connected to " + serverIP;
        }

        public override void HandleInput()
        {
            HandleRequestInput();
        }

        public override void Update(GameTime gameTime)
        {
            
        }

        protected void HandleRequestInput()
        {
            ControlInputs controlInput = Input.GetControlInput();
            switch (controlInput)
            {
                case ControlInputs.Restart:
                    Network.SendClientStart();
                    ParentControl.Mode = GameMode.Game;
                    break;

                case ControlInputs.Pause:
                    Network.SendClientJoin();
                    ParentControl.Mode = GameMode.Game;
                    break;

                default:
                    break;
            }
        }
    }
}
