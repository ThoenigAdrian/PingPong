using Microsoft.Xna.Framework;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;

namespace PingPongClient.VisualizeLayer.Lobbies
{
    class FinishLobby : LobbyInterface
    {
        public override Color GetBackgroundColor { get { return Color.White; } }

        DrawableString FinishText { get; set; } = new DrawableString("", new Vector2(), Color.Black);

        FinishLobby()
        {
            Strings.Add(FinishText);
            Strings.Add(new DrawableString("Press ESC to return to main menu.", new Vector2(), Color.Black));
        }

        public void SetFinishText(string text)
        {
            FinishText.Value = text;
        }
    }
}
