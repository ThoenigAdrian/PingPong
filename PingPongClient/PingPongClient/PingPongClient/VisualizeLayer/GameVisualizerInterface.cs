using Microsoft.Xna.Framework;
using GameLogicLibrary;
using System.Collections.Generic;
using GameLogicLibrary.GameObjects;

namespace PingPongClient.VisualizeLayer
{
    abstract class GameVisualizerInterface
    {
        GameStructure Structure { get; set; }

        List<PlayerBar> Players { get { return Structure.m_players; } }

        protected PlayBall Ball { get { return Structure.m_ball; } }

        public Vector2 FieldSize { get { return new Vector2(Structure.m_field.Width, Structure.m_field.Height); } }

        private GameVisualizerInterface() { }
        protected GameVisualizerInterface(GameStructure structure)
        {
            SetNewGameStructure(structure);
        }

        public void SetNewGameStructure(GameStructure structure)
        {
            Structure = structure;

            ApplyResize();
        }

        public void DrawGame()
        {
            DrawBegin();
            DrawBorders();
            DrawBall();
            
            foreach(PlayerBar player in Players)
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

        protected abstract void DrawPlayer(PlayerBar player);

        protected abstract void DrawEnd();
    }
}
