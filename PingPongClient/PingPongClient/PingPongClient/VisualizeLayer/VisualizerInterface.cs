using Microsoft.Xna.Framework;

namespace PingPongClient.VisualizeLayer
{
    public abstract class VisualizerInterface
    {
        public abstract void Initialize(VisualizerInitializationData initData);

        public abstract void Draw(GameTime gameTime);
    }
}
