using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PingPongClient.VisualizeLayer.Visualizers.DrawableElements
{
    public class DrawableString : DrawableElement
    {
        public Color StringColor = Color.Red;
        public string Value;
        public Vector2 Postion;

        public DrawableString(string value) : this(value, Vector2.Zero)
        {
        }

        public DrawableString(string value, Vector2 position) : this(value, position, Color.Red)
        {
        }

        public DrawableString(string value, Vector2 position, Color color)
        {
            Value = value;
            Postion = position;
            StringColor = color;
        }

        public Vector2 GetMeasurements(SpriteFont font)
        {
            return font.MeasureString(Value);
        }
    }
}
