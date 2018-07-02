using Microsoft.Xna.Framework;
using System;

namespace PingPongClient.VisualizeLayer.Visualizers.DrawableElements
{
    class WaitingAnimation : DrawableElement
    {
        VisualizerInterface Visualizer { get; set; }
        Vector2 Center { get; set; }
        int ElementCount { get; set; }
        Color AnimationColor { get; set; }
        float Offset { get; set; }
        int ElementSize { get; set; }
        float Radius { get; set; }
        float MovementWavelength { get; set; }
        float MovementStep { get; set; }
        float WavelengthMinimum { get; set; }

        public WaitingAnimation()
        {
            ElementSize = 7;
            ElementCount = 8;
            WavelengthMinimum = 30;
            Radius = WavelengthMinimum;
            AnimationColor = new Color(255, 15, 195);
            Offset = 0;

            MovementStep = 0;
            MovementWavelength = 40;
             
            //Position = new Vector2(350, 300);
        }

        public void Update()
        {
            Offset += 1.2F;
            MovementStep += 1F;
            Radius = WavelengthMinimum + (MovementWavelength / 2) + ((float)Math.Sin(GetRadiant(Offset * 1.5F)) * MovementWavelength / 2);
        }

        public void Draw(VisualizerInterface Visualizer)
        {
            Center = Visualizer.GetCenter();

            for (int i = 0; i < ElementCount; i++)
            {
                DrawSingleElement(Visualizer, i);
            }
        }

        private void DrawSingleElement(VisualizerInterface Visualizer, int index)
        {
            float angle = 360 / ElementCount * index + Offset;

            Vector2 elementPosition = new Vector2(
                Center.X + (float)Math.Cos(GetRadiant(angle)) * Radius,
                Center.Y + (float)Math.Sin(GetRadiant(angle)) * Radius);

            Color elementColor = new Color(255, 0, 128 + (int)(127 * ((float)index / ElementCount)));

            Visualizer.DrawCircle(ElementSize, elementPosition, elementColor);
        }

        private double GetRadiant(float angle)
        {
            return angle / 180 * Math.PI;
        }
    }
}
