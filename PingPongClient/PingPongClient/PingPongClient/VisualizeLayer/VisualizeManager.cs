using GameLogicLibrary.GameObjects;
using PingPongClient.ControlLayer;

namespace PingPongClient.VisualizeLayer
{
    abstract class VisualizeManager
    {
        public GameMode CurrentMode { get; set; }
        protected LobbyVisualizer LobbyVisualizer { get; set; }
        protected GameStructureVisualizer GameVisualizer { get; set; }

        public void Draw()
        {
            switch(CurrentMode)
            {
                case GameMode.Lobby:
                    LobbyVisualizer.DrawLobby();
                    break;

                case GameMode.Game:
                    GameVisualizer.DrawGame();
                    break;
            }
        }

        public void SetGameStructure(GameStructure structure)
        {
            GameVisualizer.SetGameStructure(structure);
        }

        public void SetLobby(Lobby lobby)
        {
            LobbyVisualizer.SetLobby(lobby);
        }
    }

}
