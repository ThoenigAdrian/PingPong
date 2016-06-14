using Microsoft.Xna.Framework;

namespace PingPongClient.VisualizeLayer.Lobbies.SelectionLists
{
    class StartJoinSelection : SelectionListInterface
    {
        public StartJoinSelection(Vector2 position)
        {
            TopLeft = position;
            Background = Color.Black;
        }

        protected override SelectionEntry[] CreateListEntries()
        {
            SelectionEntry[] entries = new SelectionEntry[2];
            entries[0] = new SelectionEntry(new DrawableString("Start a new game", new Vector2(0, 0), Color.White), new Selector(Color.Green, new Vector2(12,12)));
            entries[1] = new SelectionEntry(new DrawableString("Join an existing game", new Vector2(0, 30), Color.White), new Selector(Color.Green, new Vector2(12, 12)));
            return entries;
        }
    }
}
