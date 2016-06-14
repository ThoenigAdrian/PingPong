namespace GameLogicLibrary.GameObjects
{
    public class Player
    {
        public PlayerBar PlayerBar { get; set; }
        public ClientMovement PlayerMovement { get; set; }

        private Teams Team;
        private int PlayerID { get; set; }
        
        public Player(int PlayerID, Teams Team, float PositionX)
        {
            this.PlayerID = PlayerID;
            this.Team = Team;
            PlayerBar = new PlayerBar(PositionX);
        }
     }
}
