using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace PingPongClient.VisualizeLayer
{
    class XNAGameVisualizer : GameVisualizerInterface
    {
        GraphicsDeviceManager GraphicManager { get; set; }
        SpriteBatch SpriteBatchMain { get; set; }

        public override void Initialize(Game game)
        {
            GraphicManager = new GraphicsDeviceManager(game);
        }

        public override void DrawBall()
        {
            throw new NotImplementedException();
        }

        public override void DrawBorders()
        {
            throw new NotImplementedException();
        }

        public override void DrawPlayer(int playerID)
        {
            throw new NotImplementedException();
        }
    }
}
