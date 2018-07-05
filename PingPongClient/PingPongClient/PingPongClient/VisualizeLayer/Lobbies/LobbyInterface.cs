using Microsoft.Xna.Framework;
using PingPongClient.VisualizeLayer.Visualizers.DrawableElements;
using System.Collections.Generic;

namespace PingPongClient.VisualizeLayer.Lobbies
{
    abstract class LobbyInterface
    {
        protected List<DrawableString> Strings { get; set; }
        protected List<SelectionListInterface> Lists { get; set; }
        protected List<Animation> Animations { get; set; }

        public DrawableString[] GetDrawableStrings { get { return Strings.ToArray(); } }
        public SelectionListInterface[] GetSelectionLists { get { return Lists.ToArray(); } }
        public Animation[] GetAnimationList { get { return Animations.ToArray(); } }

        protected LobbyInterface()
        {
            Strings = new List<DrawableString>();
            Lists = new List<SelectionListInterface>();
            Animations = new List<Animation>();
        }

        public abstract Color GetBackgroundColor { get; }
    }
}
