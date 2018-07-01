using Microsoft.Xna.Framework;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;

namespace PingPongClient.VisualizeLayer.Lobbies
{
    class MatchmakingStatusLobby : LobbyInterface
    {
        DrawableString Status;

        public override Color GetBackgroundColor { get { return Color.Black; } }

        public MatchmakingStatusLobby()
        {
            Status = new DrawableString("", new Vector2(117, 200), Color.White);
            Strings.Add(Status);
            Strings.Add(new DrawableString("Press ESC to cancel", new Vector2(290, 350), Color.White));

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
