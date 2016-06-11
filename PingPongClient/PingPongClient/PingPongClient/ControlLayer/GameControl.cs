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
                //SendClientCommandos();
                SendMovementInputs();
                ApplyServerPositions();
            }

            Interpolation.Interpolate(gameTime);
        }

        protected void HandleControlInputs()
        {
            //if (ControlInput.GetControlInput() != ClientControls.NoInput)
            //{
            //    ClientControlPackage controlPackage = new ClientControlPackage();
            //    controlPackage.ControlInput = ControlInput.GetControlInput();
            //    Network.SendClientControl(controlPackage);
            //}

            if (Input.GetControlInput() == ControlInputs.Quit)
                ParentControl.Exit();
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

            for (int i = 0; i < Structure.m_players.Count; i++)
            {

            }

            //Structure.m_players[0].PosX = data.Player1PosX;
            //Structure.m_players[0].PosY = data.Player1PosY;

            //Structure.m_players[1].PosX = data.Player2PosX;
            //Structure.m_players[1].PosY = data.Player2PosY;

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
