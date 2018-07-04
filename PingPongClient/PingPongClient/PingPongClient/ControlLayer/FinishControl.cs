using Microsoft.Xna.Framework;

namespace PingPongClient.ControlLayer
{
    class FinishControl : SubControlInterface
    {
        public override GameMode GetMode { get { return GameMode.Finish; } }

        public FinishControl(Control parent) : base(parent)
        {
        }

        public override void Update(GameTime gameTime)
        {
        }

        public override void HandleInput()
        {
        }
    }
}
