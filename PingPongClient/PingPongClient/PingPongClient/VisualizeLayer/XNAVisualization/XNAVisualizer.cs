using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PingPongClient.VisualizeLayer.XNAVisualization
{
    public struct XNAInitializationData
    {
        public GraphicsDeviceManager GraphicManager { get; set; }
        public ContentManager Content { get; set; }
        public SpriteBatch SpriteBatch { get; set; }
    }

    public interface XNAVisualizer
    {
        void Initialize(XNAInitializationData initData);
    }
}