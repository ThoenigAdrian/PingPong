using GameLogicLibrary;
using GameLogicLibrary.GameObjects;
using Microsoft.Xna.Framework;
using NetworkLibrary.DataPackages;
using NetworkLibrary.DataPackages.ServerSourcePackages;
using PingPongClient.InputLayer;
using PingPongClient.VisualizeLayer;
using PingPongClient.VisualizeLayer.XNAVisualization;

namespace PingPongClient.ControlLayer
{
    class GameControl : SubControlInterface
    {
        GameStructure Structure { get; set; }

        Interpolation Interpolation { get; set; }

        public override GameMode GetMode { get { return GameMode.Game; } }

        public GameControl(Control parent) : base(parent)
        {
            Visualizer = new XNAStructureVisualizer();
            Structure = new GameStructure();
            Interpolation = new Interpolation(Structure);

            (Visualizer as GameStructureVisualizer).SetGameStructure(Structure);
        }

        public override void Update(GameTime gameTime)
        {
            if (Network != null)
            {
                SendClientCommandos();
                SendMovementInputs();
                ApplyServerPositions();
            }

            Interpolation.Interpolate(gameTime);
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

            for (int i = 0; i < data.PlayerList.Count; i++)
            {
                if (i >= Structure.m_players.Count)
                    break;

                Structure.m_players[i].PosX = data.PlayerList[i].PositionX;
                Structure.m_players[i].PosY = data.PlayerList[i].PositionY;
            }

            Structure.m_ball.PosX = data.BallPosX;
            Structure.m_ball.PosY = data.BallPosY;
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
