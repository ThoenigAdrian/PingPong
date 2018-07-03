using Microsoft.Xna.Framework;
using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using PingPongClient.VisualizeLayer.Lobbies;
using PingPongClient.VisualizeLayer.Visualizers;
using XSLibrary.Utility;

namespace PingPongClient.ControlLayer
{
    public class MatchmakingStatusControl : SubControlInterface
    {
        MatchmakingStatusLobby StatusLobby;
        OneShotTimer CancelTimer { get; set; }
        OneShotTimer Timeout { get; set; }
        bool Canceled { get { return CancelTimer.Started; } }

        public override GameMode GetMode { get { return GameMode.Status; } }

        public MatchmakingStatusControl(Control parent) : base(parent)
        {
            StatusLobby = new MatchmakingStatusLobby();
            Visualizer = new LobbyVisualizer(StatusLobby);
#if DEBUG
            const int CancelTime = 900;
#else
            const int CancelTime = 3;
#endif
            CancelTimer = new OneShotTimer(CancelTime * 1000* 1000, false);
            Timeout = new OneShotTimer(15 * 1000 * 1000, false);
        }

        public override void OnEnter()
        {
            CancelTimer.Reset();
            StatusLobby.ResetStatus();
            Timeout.Restart();
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

            if (Timeout == true)
                TimeoutActions();
            else
                ProcessAllTCPPackages();
        }

        private void ProcessAllTCPPackages()
        {
            if (Network == null)
                return;

            foreach (PackageInterface package in Network.GetTCPPackages())
                ProcessTCPPackage(package);
        }

        private void ProcessTCPPackage(PackageInterface package)
        {
            switch (package.PackageType)
            {
                case PackageType.ServerPlayerIDResponse:
                    HandleInitResponse(package as ServerInitializeGameResponse);
                    break;
                case PackageType.ServerMatchmakingStatusResponse:
                    HandleStatusResponse(package as ServerMatchmakingStatusResponse);
                    break;
            }
        }

        private void HandleStatusResponse(ServerMatchmakingStatusResponse statusResponse)
        {
            StatusLobby.SetStatus(statusResponse.Status);

            if (statusResponse.Error)
                IntiializeDelayedCancel();
            else
            {
                if (statusResponse.GameFound)
                    StatusLobby.SetSuccess();

                Timeout.Restart();
            }
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

        private void TimeoutActions()
        {
            StatusLobby.SetStatus("Timeout!");
            IntiializeDelayedCancel();
        }

        private void IntiializeDelayedCancel()
        {
            StatusLobby.SetError();
            Timeout.Reset();
            CancelTimer.Restart();
        }

        private void Cancel()
        {
            ParentControl.Disconnect();
        }
    }
}
