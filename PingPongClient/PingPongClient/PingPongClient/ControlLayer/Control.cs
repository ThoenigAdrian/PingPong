using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using InputFunctionality.KeyboardAdapter;
using PingPongClient.NetworkLayer;
using System.Net;
using NetworkLibrary.DataStructs;

namespace PingPongClient
{
    public class Control : Game
    {
        LogWriter Logger = new LogWriter();
        NetworkUDP UDPNetwork = new NetworkUDP(IPAddress.Parse("127.0.0.1"));
        KeyboardAdvanced Input = new KeyboardAdvanced();
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public Control()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            Input.Initialize();
            UDPNetwork.Logger = Logger;
            UDPNetwork.Connect();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void UnloadContent()
        {
            UDPNetwork.Disconnect();
        }

        protected override void Update(GameTime gameTime)
        {
            Input.UpdateState();

            if(Input.KeyNowPressed(Keys.Escape))
                this.Exit();

            ServerDataUDP data = UDPNetwork.Receive();
            Logger.Log(Convert.ToString(data.TestValue));

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            base.Draw(gameTime);
        }
    }
}
