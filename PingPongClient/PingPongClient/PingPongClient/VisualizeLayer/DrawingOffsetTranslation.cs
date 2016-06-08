using Microsoft.Xna.Framework;

namespace PingPongClient.VisualizeLayer
{
    class DrawingOffsetTranslation
    {
        public Vector2 DrawingOffset { get; set; }
        public float Scaling { get; set; }

        public DrawingOffsetTranslation()
        {
            DrawingOffset = new Vector2(0, 0);
            Scaling = 1;
        }

        public Vector2 GetAbsolutePoint(Vector2 relativePoint)
        {
            return new Vector2(GetAbsoluteX(relativePoint.X), GetAbsoluteY(relativePoint.Y));
        }

        public float GetAbsoluteX(float relativeX)
        {
            return DrawingOffset.X + (relativeX * Scaling);
        }

        public float GetAbsoluteY(float relativeY)
        {
            return DrawingOffset.Y + (relativeY * Scaling);
        }

        public float GetAbsoluteSize(float size)
        {
            return size * Scaling;
        }
    }
}
