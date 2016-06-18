using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;
using Microsoft.Xna.Framework;

namespace PingPongClient.VisualizeLayer.Lobbies.SelectionLists
{
    class RegistrationOptions : SelectionListInterface
    {
        protected override SelectionEntry[] CreateInitialListEntries()
        {
            SelectionEntry[] entries = new SelectionEntry[1];
            entries[0] = new SelectionEntry(new DrawableString("Ready!", new Vector2(100, 250), Color.White), new Selector());
            return entries;
        }
    }
}
