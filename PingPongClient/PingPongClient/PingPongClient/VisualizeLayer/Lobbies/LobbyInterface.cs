using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PingPongClient.VisualizeLayer.Lobbies.SelectionLists;
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


    public class DrawableString
    {
        public Color StringColor = Color.Red;
        public string Value;
        public Vector2 Postion;

        public DrawableString(string value) : this (value, Vector2.Zero)
        {
        }

        public DrawableString(string value, Vector2 position) : this (value, position, Color.Red)
        {
        }

        public DrawableString(string value, Vector2 postion, Color color)
        {
            Value = value;
            Postion = postion;
            StringColor = color;
        }

        public Vector2 GetMeasurements(SpriteFont font)
        {
            return font.MeasureString(Value);
        }
    }
}
