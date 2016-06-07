using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace PingPongClient.VisualizeLayer
{
    class XNAGameVisualizer : GameVisualizerInterface
    {
        GraphicsDeviceManager GraphicManager { get; set; }
        SpriteBatch SpriteBatchMain { get; set; }
        ContentManager Content { get; set; }
        GraphicsDevice Graphics { get { return GraphicManager.GraphicsDevice; } }

        Vector2 DrawingOffset;

        Texture2D Border;
        Texture2D Ball;
        Texture2D Player;

        public override void Initialize(Game game)
        {
            GraphicManager = new GraphicsDeviceManager(game);
            Content = new ContentManager(game.Services);
            Content.RootDirectory = "Content";

            DrawingOffset = new Vector2(100, 100);
        }

        public override void LoadContent()
        {
            SpriteBatchMain = new SpriteBatch(Graphics);

            Border = new Texture2D(Graphics, 1, 1);
            Border.SetData(new Color[] { Color.White });

            Ball = createCircleText(100);

            Player = new Texture2D(Graphics, 1, 1);
            Player.SetData(new Color[] { Color.White });
        }

        public override void DrawBegin()
        {
            Graphics.Clear(Color.Black);
            SpriteBatchMain.Begin();
        }

        public override void DrawBall()
        {
            int BallPosX = (int)GetAbsoluteX(BallPosition.X - BallRadius);
            int BallPosY = (int)GetAbsoluteX(BallPosition.Y - BallRadius);
            SpriteBatchMain.Draw(Ball, new Rectangle(BallPosX, BallPosY, BallRadius * 2, BallRadius * 2), Color.Black);
        }

        public override void DrawBorders()
        {
            SpriteBatchMain.Draw(Border, new Rectangle((int)GetAbsoluteX(0), (int)GetAbsoluteY(0), (int)BorderSize.X, (int)BorderSize.Y), Color.White);
        }

        public override void DrawPlayer(int playerID)
        {
            PlayerDrawingData playerData = PlayerData[playerID];

            int PlayerPosX = (int)GetAbsoluteX(PlayerData[playerID].Position.X);
            int PlayerPosY = (int)GetAbsoluteY(PlayerData[playerID].Position.Y);

            SpriteBatchMain.Draw(Player, new Rectangle(PlayerPosX, PlayerPosY, (int)playerData.BorderSize.X, (int)playerData.BorderSize.Y), Color.Black);
        }

        public override void DrawEnd()
        {
            SpriteBatchMain.End();
        }

        Vector2 GetAbsolutePoint(Vector2 relativePoint)
        {
            return new Vector2(DrawingOffset.X + relativePoint.X, DrawingOffset.Y + relativePoint.Y);
        }

        float GetAbsoluteX(float relativeX)
        {
            return DrawingOffset.X + relativeX;
        }

        float GetAbsoluteY(float relativeY)
        {
            return DrawingOffset.Y + relativeY;
        }

        Texture2D createCircleText(int radius)
        {
            Texture2D texture = new Texture2D(Graphics, radius, radius);
            Color[] colorData = new Color[radius * radius];

            float diam = radius / 2f;
            float diamsq = diam * diam;

            for (int x = 0; x < radius; x++)
            {
                for (int y = 0; y < radius; y++)
                {
                    int index = x * radius + y;
                    Vector2 pos = new Vector2(x - diam, y - diam);
                    if (pos.LengthSquared() <= diamsq)
                    {
                        colorData[index] = Color.White;
                    }
                    else
                    {
                        colorData[index] = Color.Transparent;
                    }
                }
            }

            texture.SetData(colorData);
            return texture;
        }
    }
}
