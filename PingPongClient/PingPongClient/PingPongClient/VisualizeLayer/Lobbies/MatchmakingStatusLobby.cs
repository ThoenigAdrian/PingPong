using Microsoft.Xna.Framework;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;

namespace PingPongClient.VisualizeLayer.Lobbies
{
    class MatchmakingStatusLobby : LobbyInterface
    {
        DrawableString Status;
        WaitingAnimation Animation { get; set; }

        public override Color GetBackgroundColor { get { return Color.Black; } }

        public MatchmakingStatusLobby()
        {
            Status = new DrawableString("", new Vector2(117, 100), Color.White);
            Strings.Add(Status);
            Strings.Add(new DrawableString("Best regards, Sara!", new Vector2(290, 350), new Color(255, 0, 200)));
            Strings.Add(new DrawableString("Press ESC to cancel", new Vector2(290, 380), Color.White));
            Animations.Add(new WaitingAnimation());

            ResetStatus();
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
        }
    }
}
