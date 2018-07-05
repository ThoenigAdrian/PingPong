using Microsoft.Xna.Framework;

namespace PingPongClient.VisualizeLayer.Visualizers.DrawableElements
{
    abstract class Animation : DrawableElement
    {
        public Vector2 Center;

        public abstract void Update();
        public abstract void Draw(VisualizerInterface visualizer);
        public abstract void Reset();
    }
}
