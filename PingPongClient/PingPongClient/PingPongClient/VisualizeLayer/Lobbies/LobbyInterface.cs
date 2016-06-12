using System;
using Microsoft.Xna.Framework;

namespace PingPongClient.VisualizeLayer.Lobbies
{
    abstract class LobbyInterface
    {
        public abstract DrawableString[] GetDrawableStrings { get; }

        public abstract Color GetBackgroundColor { get; }
    }


    public class DrawableString
    {
        public Color StringColor = Color.Red;
        public string Value;
        public float PosX = 0;
        public float PosY = 0;

        public DrawableString(string value)
        {
            Value = value;
        }
    }
}
