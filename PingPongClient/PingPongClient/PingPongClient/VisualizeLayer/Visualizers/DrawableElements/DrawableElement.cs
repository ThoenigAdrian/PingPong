namespace PingPongClient.VisualizeLayer.Visualizers.DrawableElements
{
    public abstract class DrawableElement
    {
        public bool Visible { get; set; }

        public DrawableElement()
        {
            Visible = true;
        }
    }
}
