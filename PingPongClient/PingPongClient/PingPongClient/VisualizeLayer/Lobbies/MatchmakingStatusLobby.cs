using Microsoft.Xna.Framework;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;

namespace PingPongClient.VisualizeLayer.Lobbies
{
    class MatchmakingStatusLobby : LobbyInterface
    {
        DrawableString Status { get; set; }
        WaitingAnimation Animation { get; set; }

        public override Color GetBackgroundColor { get { return Color.Black; } }

        public MatchmakingStatusLobby()
        {
            Status = new DrawableString("", new Vector2(117, 100), Color.White);
            Strings.Add(Status);
            Strings.Add(new DrawableString("Press ESC to cancel", new Vector2(290, 380), Color.White));

            Animation = new WaitingAnimation();
            Animations.Add(Animation);

            ResetStatus();
        }

        public void SetSuccess()
        {
            if(Animation.AnimationState != State.Success)
                Animation.AnimateSuccess();
        }

        public void SetError()
        {
            Animation.AnimateError();
        }

        public void SetStatus(string status)
        {
            Status.Value = status;
            Status.Visible = true;
        }

        public void ResetStatus()
        {
            Status.Value = "Waiting for match...";
            Status.Visible = true;
            Animation.ResetState();
        }
    }
}
