namespace PingPongClient.VisualizeLayer.Visualizers.DrawableElements
{
    abstract class Animation : DrawableElement
    {
        public abstract void Update();
        public abstract void Draw(VisualizerInterface Visualizer);
    }
}
