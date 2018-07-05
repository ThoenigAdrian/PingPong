using Microsoft.Xna.Framework;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;

namespace PingPongClient.VisualizeLayer.Lobbies.SelectionLists
{
    class QueueSelection : SelectionListInterface
    {
        public QueueSelection(Vector2 position)
        {
            TopLeft = position;
            Background = Color.Black;
        }

        protected override SelectionEntry[] CreateInitialListEntries()
        {
            SelectionEntry[] entries = new SelectionEntry[2];

            entries[0] = new SelectionEntry(
                new DrawableString(
                    new DrawingOptions() { Position = new Vector2(0, 0) }, 
                    "Queue up for matchmaking"),
                new Selector());

            entries[1] = new SelectionEntry(
                new DrawableString(
                    new DrawingOptions() { Position = new Vector2(0, 30) },
                    "Observe a game"),
                new Selector());

            return entries;
        }
    }
}
