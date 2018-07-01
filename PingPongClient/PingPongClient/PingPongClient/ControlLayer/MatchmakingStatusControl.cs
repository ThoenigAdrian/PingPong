using Microsoft.Xna.Framework;
using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using NetworkLibrary.Utility;
using PingPongClient.VisualizeLayer.Lobbies;
using PingPongClient.VisualizeLayer.Visualizers;

namespace PingPongClient.ControlLayer
{
    public class MatchmakingStatusControl : SubControlInterface
    {
        MatchmakingStatusLobby StatusLobby;
        OneShotTimer CancelTimer { get; set; }
        bool Canceled { get { return CancelTimer.Started; } }

        public override GameMode GetMode { get { return GameMode.Status; } }

        public MatchmakingStatusControl(Control parent) : base(parent)
        {
            StatusLobby = new MatchmakingStatusLobby();
            Visualizer = new LobbyVisualizer(StatusLobby);
            CancelTimer = new OneShotTimer(3 * 1000* 1000, false);
        }

        public override void OnEnter()
        {
            CancelTimer.Reset();
            StatusLobby.ResetStatus();
            IssueServerResponse(PackageType.ServerMatchmakingStatusResponse, 15000);
        }

        public override void HandleInput()
        {
        }

        public override void Update(GameTime gameTime)
        {
            if(Canceled && CancelTimer == true)
            {
                CancelTimer.Reset();
                Cancel();
            }
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
            StatusLobby.SetStatus(statusResponse.Status);

            if (statusResponse.Error)
                IntiializeDelayedCancel();
            else if (statusResponse.GameFound)
                IssueServerResponse(PackageType.ServerPlayerIDResponse);
            else
                IssueServerResponse(PackageType.ServerMatchmakingStatusResponse, 15000);
        }

        private void HandleInitResponse(ServerInitializeGameResponse initResponse)
        {
            if (initResponse.m_players == null || initResponse.m_ball == null || initResponse.m_field == null)
            {
                IntiializeDelayedCancel();
                StatusLobby.SetStatus("Invalid server response!");
                return;
            }

            ParentControl.GameControl.InitializeGame(initResponse.m_players, initResponse.m_field, initResponse.m_ball);
            ParentControl.SwitchMode(GameMode.Game);
        }

        protected override void ResponseTimeoutActions(PackageType requestedPackageType)
        {
            StatusLobby.SetStatus("Timeout!");
            IntiializeDelayedCancel();
        }

        private void IntiializeDelayedCancel()
        {
            CancelTimer.Restart();
        }

        private void Cancel()
        {
            ParentControl.Disconnect();
        }
    }
}
