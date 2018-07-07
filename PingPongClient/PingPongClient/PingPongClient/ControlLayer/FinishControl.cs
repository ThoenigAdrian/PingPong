using Microsoft.Xna.Framework;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using PingPongClient.VisualizeLayer.Lobbies;
using PingPongClient.VisualizeLayer.Visualizers;

namespace PingPongClient.ControlLayer
{
    public class FinishControl : SubControlInterface
    {
        public override GameMode GetMode { get { return GameMode.Finish; } }

        FinishLobby Lobby;

        public FinishControl(Control parent) : base(parent)
        {
            Lobby = new FinishLobby();
            Visualizer = new LobbyVisualizer(Lobby);
        }

        public override void OnEnter()
        {
            ParentControl.Disconnect();
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void HandleInput()
        {
        }

        public void ProcessFinishPackage(ServerGameControlPackage package)
        {
            if(package.Winner == GameLogicLibrary.Teams.Team1)
                Lobby.SetFinishText(string.Format("Team 1 won {0}-{1}", package.Score.Team1, package.Score.Team2));
            else
                Lobby.SetFinishText(string.Format("Team 2 won {0}-{1}", package.Score.Team2, package.Score.Team1));
        }
    }
}
