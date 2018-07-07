using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PingPongClient.VisualizeLayer.Visualizers.DrawableElements
{
    public class DrawingOptions
    {
        public Color StringColor = Color.White;
        public Vector2 Position = new Vector2();
        public bool DrawCentered = false;
    }

    public class DrawableString : DrawableElement
    {
        public DrawingOptions Options;
        public string Value = "";

        public DrawableString() : this (new DrawingOptions())
        {
        }

        public DrawableString(DrawingOptions options)
        {
            Options = options;
        }

        public DrawableString(DrawingOptions options, string value)
        {
            Options = options;
            Value = value;
        }

        public Vector2 GetMeasurements(SpriteFont font)
        {
            return font.MeasureString(Value);
        }
    }
}
