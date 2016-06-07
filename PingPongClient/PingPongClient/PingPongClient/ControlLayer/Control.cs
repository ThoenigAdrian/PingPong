using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Net;
using PingPongClient.InputLayer;
using PingPongClient.NetworkLayer;
using NetworkLibrary.NetworkImplementations;
using NetworkLibrary.Utility;
using NetworkLibrary;
using GameLogicLibrary;
using NetworkLibrary.DataStructs;

namespace PingPongClient
{
    public class Control : Game
    {
        ConnectionClient Connection { get; set; }
        LogWriter Logger = new LogWriterConsole();
        InputInterface Input = new KeyboardInput();
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Control()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            IPEndPoint server = new IPEndPoint(IPAddress.Parse("127.0.0.1"), NetworkConstants.SERVER_PORT);

            Input.Initialize();
            Connection = new ConnectionClient(server);
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
            
        }

        protected override void Update(GameTime gameTime)
        {
            Input.Update();

            if(Input.GetInput() == ClientControls.Quit)
                this.Exit();

            ServerDataPackage data = Connection.GetServerData();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }
    }
}
