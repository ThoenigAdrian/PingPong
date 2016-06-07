namespace GameLogicLibrary.GameObjects
{
    public class GameField
    {
        public int Height { get; set; }
        public int Width { get; set; }

        public GameField()
        {
            Height = GameInitializers.BORDER_HEIGHT;
            Width = GameInitializers.BORDER_WIDTH;
        }
    }
}
