namespace GameLogicLibrary.GameObjects
{
    public class RawPlayer
    {
        public int ID { get; set; }

        public float PositionX { get; set; }
        public float PositionY { get; set; }

        public float DirectionY { get; set; }
    }


    public class Player : RawPlayer
    {
        public bool Controllable { get; set; }
        public int Team { get; set; }
        
        public float Height { get; set; }
        public float Width { get; set; }

        public float Speed { get; set; }

        public Player(int playerID, int team, float positionX)
        {
            ID = playerID;
            Team = team;
            Controllable = false;

            PositionX = positionX;
            PositionY = GameInitializers.GetPlayerY(0);

            Height = GameInitializers.GetPlayerHeight(0);
            Width = GameInitializers.PLAYER_WIDTH;

            DirectionY = 0;
            Speed = GameInitializers.PLAYER_SPEED;
        }

        public override string ToString()
        {
            return "Player " + ID.ToString();
        }
    }
}
