using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace PingPongClient.VisualizeLayer.Visualizers
{
    public struct XNAInitializationData
    {
        public GraphicsDeviceManager GraphicManager { get; set; }
        public ContentManager Content { get; set; }
        public SpriteBatch SpriteBatch { get; set; }
    }
}