using GameLogicLibrary;
using GameLogicLibrary.GameObjects;
using Microsoft.Xna.Framework;
using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using PingPongClient.InputLayer;
using PingPongClient.InputLayer.KeyboardInputs;
using PingPongClient.VisualizeLayer.Visualizers;
using System.Collections.Generic;

namespace PingPongClient.ControlLayer
{
    public class GameControl : SubControlInterface
    {
        BasicStructure Structure { get; set; }
        Ball Ball { get { return Structure.Ball; } }
        GameField Field { get { return Structure.Field; } }
        List<Player> Players { get { return Structure.Players; } }

        Interpolation Interpolation { get; set; }

        public override GameMode GetMode { get { return GameMode.Game; } }


        public GameControl(Control parent) : base(parent)
        {
            Visualizer = new GameStructureVisualizer();
            Structure = null;
            Interpolation = new Interpolation();

            (Visualizer as GameStructureVisualizer).SetGameStructure(Structure);
        }

        public override void HandleInput()
        {
            if (Network != null)
            {
                SendClientCommandos();
                SendMovementInputs();
            }
        }

        public override void Update(GameTime gameTime)
        {
            if (Network != null)
            {
                ApplyServerPositions();
                UpdateScore();
            }

            Interpolation.Interpolate(gameTime);
        }

        public void InitializeGame(Player[] players, GameField field, Ball ball)
        {
            if (Network != null)
                Network.StartHolePunching();

            Structure = new BasicStructure(field, ball);

            Input.ClearPlayerInput();

            int index = 0;
            foreach (Player player in players)
            {
                if (player.Controllable)
                    AddControllablePlayer(player, index++);
                else
                    AddPlayer(player);
            }

            (Visualizer as GameStructureVisualizer).SetGameStructure(Structure);
        }

        private void AddControllablePlayer(Player player, int index)
        {
            Structure.Players.Add(player);
            Input.AddPlayerInput(player.ID, index++);
        }

        private void AddPlayer(Player player)
        {
            Structure.Players.Add(player);
        }

        protected void SendMovementInputs()
        {
            PlayerInputs[] playerInputs = Input.GetMovementInput();

            foreach (PlayerInputs inputs in playerInputs)
            {
                if (inputs.MovementInput != PlayerMovementInputs.NoInput)
                {
                    PlayerMovementPackage movementPackage = new PlayerMovementPackage();
                    movementPackage.PlayerID = inputs.ID;

                    switch (inputs.MovementInput)
                    {
                        case PlayerMovementInputs.Up:
                            movementPackage.PlayerMovement = ClientMovement.Up;
                            break;

                        case PlayerMovementInputs.Down:
                            movementPackage.PlayerMovement = ClientMovement.Down;
                            break;

                        case PlayerMovementInputs.StopMoving:
                            movementPackage.PlayerMovement = ClientMovement.StopMoving;
                            break;
                    }

                    Network.SendPlayerMovement(movementPackage);
                }
            }
        }

        protected void ApplyServerPositions()
        {
            ServerDataPackage data = Network.GetServerData();

            if (data == null)
                return;

            for (int i = 0; i < data.Players.Count; i++)
            {
                Player localPlayer;
                RawPlayer serverPlayer = data.Players[i];

                if ((localPlayer = GetPlayerWithID(serverPlayer.ID)) == null)
                    continue;

                localPlayer.PositionX = serverPlayer.PositionX;
                localPlayer.PositionY = serverPlayer.PositionY;
            }

            Structure.Ball.PositionX = data.Ball.PositionX;
            Structure.Ball.PositionY = data.Ball.PositionY;
        }

        protected void UpdateScore()
        {
            ServerGameControlPackage score = Network.GetScore();
            if (score == null)
                return;

            Structure._score.Score_Team1 = score.Score.Team1;
            Structure._score.Score_Team2 = score.Score.Team2;
        }

        private Player GetPlayerWithID(int ID)
        {
            foreach (Player player in Players)
            {
                if (player.ID == ID)
                    return player;
            }

            return null;
        }

        protected void SendClientCommandos()
        {
            //if (ControlInput.GetControlInput() == ClientControls.Restart)
            //{
            //    Network.SendUDPTestData(new PlayerMovementPackage());
            //}
        }
    }
}
