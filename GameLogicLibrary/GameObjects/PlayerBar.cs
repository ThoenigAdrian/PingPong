namespace GameLogicLibrary.GameObjects
{
    public class PlayerBar
    {
        public float PositionX { get; set; }
        public float PositionY { get; set; }

        public float Height { get; set; }
        public float Width { get; set; }

        
        public PlayerBar(float PositionX)
        {
            this.PositionX = PositionX;
            PositionY = GameInitializers.PLAYER_Y;

            Height = GameInitializers.PLAYER_HEIGHT;
            Width = GameInitializers.PLAYER_WIDTH;
            
        }
    }
}
