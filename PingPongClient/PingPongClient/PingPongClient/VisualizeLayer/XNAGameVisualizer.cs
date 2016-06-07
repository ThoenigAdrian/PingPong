using GameLogicLibrary.GameObjects;
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

        public XNAGameVisualizer(GameStructure structure) : base(structure)
        {

        }

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

            Ball = CreateCircleTexture(100);

            Player = new Texture2D(Graphics, 1, 1);
            Player.SetData(new Color[] { Color.White });
        }

        public override void ApplyResize()
        {
            int screenHeight = GraphicManager.PreferredBackBufferHeight;
            int screenWidth = GraphicManager.PreferredBackBufferWidth;
            DrawingOffset.X = screenWidth / 2 - FieldSize.X / 2;
            DrawingOffset.Y = screenHeight / 2 - FieldSize.Y / 2;
        }

        protected override void DrawBegin()
        {
            Graphics.Clear(Color.Black);
            SpriteBatchMain.Begin();
        }

        protected override void DrawBall()
        {
            int BallPosX = (int)GetAbsoluteX(BallPosition.X - BallRadius);
            int BallPosY = (int)GetAbsoluteY(BallPosition.Y - BallRadius);

            SpriteBatchMain.Draw(Ball, new Rectangle(BallPosX, BallPosY, BallRadius * 2, BallRadius * 2), Color.Black);
        }

        protected override void DrawBorders()
        {
            SpriteBatchMain.Draw(Border, new Rectangle((int)GetAbsoluteX(0), (int)GetAbsoluteY(0), (int)FieldSize.X, (int)FieldSize.Y), Color.White);
        }

        protected override void DrawPlayer(PlayerSlot player)
        {
            int PlayerPosX = (int)GetAbsoluteX(GetPlayerPosition(player).X);
            int PlayerPosY = (int)GetAbsoluteY(GetPlayerPosition(player).Y);

            SpriteBatchMain.Draw(Player, new Rectangle(PlayerPosX, PlayerPosY, (int)GetPlayerBorder(player).X, (int)GetPlayerBorder(player).Y), Color.Black);
        }

        protected override void DrawEnd()
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

        Texture2D CreateCircleTexture(int radius)
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
