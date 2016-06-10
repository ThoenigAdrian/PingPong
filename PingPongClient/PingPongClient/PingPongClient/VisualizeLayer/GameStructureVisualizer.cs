using Microsoft.Xna.Framework;
using GameLogicLibrary;
using System.Collections.Generic;
using GameLogicLibrary.GameObjects;

namespace PingPongClient.VisualizeLayer
{
    abstract class GameStructureVisualizer
    {
        GameStructure Structure { get; set; }

        List<PlayerBar> Players { get { return Structure.m_players; } }

        protected PlayBall Ball { get { return Structure.m_ball; } }

        public Vector2 FieldSize { get { return new Vector2(Structure.m_field.Width, Structure.m_field.Height); } }

        public void SetGameStructure(GameStructure structure)
        {
            Structure = structure;

            ApplyResize();
        }

        public void DrawGame()
        {
            if (Structure == null || !CanDraw())
                return;

            DrawBegin();
            DrawBorders();
            DrawBall();
            
            foreach(PlayerBar player in Players)
            {
                DrawPlayer(player);
            }

            DrawEnd();
        }

        public abstract void LoadContent();

        public abstract void ApplyResize();

        protected abstract bool CanDraw();

        protected abstract void DrawBegin();

        protected abstract void DrawBorders();

        protected abstract void DrawBall();

        protected abstract void DrawPlayer(PlayerBar player);

        protected abstract void DrawEnd();
    }
}
