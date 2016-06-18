using Microsoft.Xna.Framework;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;

namespace PingPongClient.VisualizeLayer.Lobbies.SelectionLists
{
    class ConnectOptions : SelectionListInterface
    {
        protected override SelectionEntry[] CreateListEntries()
        {
            SelectionEntry[] entries = new SelectionEntry[2];
            entries[0] = new SelectionEntry(new DrawableString("Connect with a new session", new Vector2(0, 0), Color.White), new Selector());
            entries[1] = new SelectionEntry(new DrawableString("Reconnect with previous session", new Vector2(0, 30), Color.White), new Selector());
            return entries;
        }
    }
}
