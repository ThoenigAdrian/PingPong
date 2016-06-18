using Microsoft.Xna.Framework;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;
using System.Collections.Generic;

namespace PingPongClient.VisualizeLayer.Lobbies
{
    abstract class LobbyInterface
    {
        protected List<DrawableString> Strings { get; set; }
        protected List<SelectionListInterface> Lists { get; set; }

        public DrawableString[] GetDrawableStrings { get { return Strings.ToArray(); } }
        public SelectionListInterface[] GetSelectionLists { get { return Lists.ToArray(); } }

        protected LobbyInterface()
        {
            Strings = new List<DrawableString>();
            Lists = new List<SelectionListInterface>();
        }

        public abstract Color GetBackgroundColor { get; }
    }
}
