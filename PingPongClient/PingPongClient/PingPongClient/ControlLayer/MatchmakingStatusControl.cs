using Microsoft.Xna.Framework;
using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.DataPackages.ServerSourcePackages.Matchmaking;
using PingPongClient.InputLayer.KeyboardInputs;
using PingPongClient.VisualizeLayer.Lobbies;
using PingPongClient.VisualizeLayer.Visualizers;

namespace PingPongClient.ControlLayer
{
    public class MatchmakingStatusControl : SubControlInterface
    {
        MatchmakingStatusLobby StatusLobby;

        public override GameMode GetMode { get { return GameMode.Status; } }

        public MatchmakingStatusControl(Control parent) : base(parent)
        {
            StatusLobby = new MatchmakingStatusLobby();
            Visualizer = new LobbyVisualizer(StatusLobby);
        }

        public override void HandleInput()
        {
            if(Input.GetTextEditInput() == TextEditInputs.Delete)
                CancelWaiting();
        }

        public override void Update(GameTime gameTime)
        {
            // maybe animation here
        }

        protected override void ServerResponseActions(PackageInterface responsePackage)
        {
            ServerMatchmakingStatusResponse statusResponse = responsePackage as ServerMatchmakingStatusResponse;
            if (statusResponse != null)
            {
                HandleStatusResponse(statusResponse);
                return;
            }

            ServerInitializeGameResponse initResponse = responsePackage as ServerInitializeGameResponse;
            if (initResponse != null)
            {
                HandleInitResponse(initResponse);
                return;
            }
        }

        private void HandleStatusResponse(ServerMatchmakingStatusResponse statusResponse)
        {
            if (statusResponse.Error)
                CancelWaiting();

            StatusLobby.SetStatus(statusResponse.Status);

            if (statusResponse.GameFound)
                IssueServerResponse(PackageType.ServerPlayerIDResponse);
            else
                IssueServerResponse(PackageType.ServerMatchmakingStatusResponse, 15000);
        }

        private void HandleInitResponse(ServerInitializeGameResponse initResponse)
        {
            if (initResponse.m_players == null || initResponse.m_ball == null || initResponse.m_field == null)
            {
                CancelWaiting();
                //ParentControl.RegistrationControl.RegistrationLobby.SetStatus("Server response invalid!");
                return;
            }

            ParentControl.GameControl.InitializeGame(initResponse.m_players, initResponse.m_field, initResponse.m_ball);
            ParentControl.SwitchMode(GameMode.Game);
        }

        protected override void ResponseTimeoutActions(PackageType requestedPackageType)
        {
            CancelWaiting();
        }

        private void CancelWaiting()
        {

        }
    }
}
