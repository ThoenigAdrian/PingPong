using Microsoft.Xna.Framework;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;

namespace PingPongClient.VisualizeLayer.Lobbies.SelectionLists
{
    class ConnectOptions : SelectionListInterface
    {
        protected override SelectionEntry[] CreateInitialListEntries()
        {
            SelectionEntry[] entries = new SelectionEntry[2];

            entries[0] = new SelectionEntry(
                new DrawableString(
                    new DrawingOptions() { Position = new Vector2(0, 0) },
                    "Connect with a new session"),
                new Selector());

            entries[1] = new SelectionEntry(
                new DrawableString(
                    new DrawingOptions() { Position = new Vector2(0, 30) },
                    "Reconnect with previous session"),
                new Selector());

            return entries;
        }
    }
}
