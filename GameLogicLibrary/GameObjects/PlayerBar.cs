namespace GameLogicLibrary.GameObjects
{
    public class PlayerBar
    {
        public float PosX { get; set; }
        public float PosY { get; set; }

        public float Height { get; set; }
        public float Width { get; set; }

        public PlayerBar(float posX)
        {
            PosX = posX;
            PosY = GameInitializers.PLAYER_Y;

            Height = GameInitializers.PLAYER_HEIGHT;
            Width = GameInitializers.PLAYER_WIDTH;
        }
    }
}
