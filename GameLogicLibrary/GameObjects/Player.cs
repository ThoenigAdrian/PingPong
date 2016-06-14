namespace GameLogicLibrary.GameObjects
{
    public class Player
    {
        public PlayerBar PlayerBar { get; set; }
        public ClientMovement PlayerMovement { get; set; }

        private int TeamNumber;
        private int PlayerID { get; set; }
        
        public Player(int PlayerID, int TeamNumber, float PositionX)
        {
            this.PlayerID = PlayerID;
            this.TeamNumber = TeamNumber;
            PlayerBar = new PlayerBar(PositionX);
        }
     }
}
