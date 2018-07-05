using Microsoft.Xna.Framework;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;

namespace PingPongClient.VisualizeLayer.Lobbies
{
    class FinishLobby : LobbyInterface
    {
        public override Color GetBackgroundColor { get { return Color.White; } }

        DrawableString FinishText { get; set; }

        public FinishLobby()
        {
            DrawingOptions options = new DrawingOptions() { Position = new Vector2(305, 150), StringColor = Color.Black, DrawCentered = true };
            FinishText = new DrawableString(options);
            Strings.Add(FinishText);

            options = new DrawingOptions() { Position = new Vector2(220, 350), StringColor = Color.Black, DrawCentered = true };
            Strings.Add(new DrawableString(options, "Press ESC to return to main menu"));

            FinishAnimation animation = new FinishAnimation(new Vector2(0, 250));
            Animations.Add(animation);
        }

        public void SetFinishText(string text)
        {
            FinishText.Value = text;
        }
    }
}
