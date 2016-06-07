using Microsoft.Xna.Framework;
using GameLogicLibrary;
using System.Collections.Generic;
using GameLogicLibrary.GameObjects;

namespace PingPongClient.VisualizeLayer
{
    abstract class GameVisualizerInterface
    {
        GameStructure Structure { get; set; }

        const int PLAYER_COUNT = 2;

        protected enum PlayerSlot
        {
            Player1,
            Player2
        }

        protected Vector2 BallPosition { get { return new Vector2(Structure.m_ball.PosX, Structure.m_ball.PosY); } }
        protected int BallRadius { get { return Structure.m_ball.Radius; } }

        public Vector2 FieldSize { get { return new Vector2(Structure.m_field.Width, Structure.m_field.Height); } }

        protected Vector2 GetPlayerPosition(PlayerSlot player)
        {
            switch(player)
            {
                case PlayerSlot.Player1:
                    return new Vector2(Structure.m_player1.PosX, Structure.m_player1.PosY);
                case PlayerSlot.Player2:
                    return new Vector2(Structure.m_player2.PosX, Structure.m_player2.PosY);
            }

            return Vector2.Zero;
        }

        protected Vector2 GetPlayerBorder(PlayerSlot player)
        {
            switch (player)
            {
                case PlayerSlot.Player1:
                    return new Vector2(Structure.m_player1.Width, Structure.m_player1.Height);
                case PlayerSlot.Player2:
                    return new Vector2(Structure.m_player2.Width, Structure.m_player2.Height);
            }

            return Vector2.Zero;
        }

        private GameVisualizerInterface() { }
        protected GameVisualizerInterface(GameStructure structure)
        {
            Structure = structure;
        }

        public void DrawGame()
        {
            DrawBegin();
            DrawBorders();
            DrawBall();
            
            foreach(PlayerSlot player in PlayerSlot.GetValues(typeof(PlayerSlot)))
            { 
                DrawPlayer(player);
            }

            DrawEnd();
        }

        public abstract void Initialize(Game game);

        public abstract void LoadContent();

        public abstract void ApplyResize();

        protected abstract void DrawBegin();

        protected abstract void DrawBorders();

        protected abstract void DrawBall();

        protected abstract void DrawPlayer(PlayerSlot player);

        protected abstract void DrawEnd();
    }

    class PlayerDrawingData
    {
        public Vector2 Position { get; set; }
        public Vector2 BorderSize { get; set; }
    }
}
