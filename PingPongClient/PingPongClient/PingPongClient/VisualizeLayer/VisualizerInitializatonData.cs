using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PingPongClient.VisualizeLayer
{
    public interface VisualizerInitializationData
    {

    }

    public struct XNAInitializationData : VisualizerInitializationData
    {
        public GraphicsDeviceManager GraphicManager { get; set; }
        public ContentManager Content { get; set; }
        public SpriteBatch SpriteBatch { get; set; }
    }
}