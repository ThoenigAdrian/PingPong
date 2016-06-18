using Microsoft.Xna.Framework;
using NetworkLibrary.DataPackages;
using PingPongClient.InputLayer;
using PingPongClient.NetworkLayer;
using PingPongClient.VisualizeLayer.Visualizers;

namespace PingPongClient.ControlLayer
{
    public abstract class SubControlInterface
    {
        protected Control ParentControl { get; set; }
        protected VisualizerInterface Visualizer { get; set; }
        public bool WaitingForResponse { get; private set; }

        protected ClientNetwork Network
        {
            get { return ParentControl.Network; }
            set { ParentControl.Network = value; }
        }

        protected InputManager Input { get { return ParentControl.InputManager; } }

        public SubControlInterface(Control parent)
        {
            ParentControl = parent;
            WaitingForResponse = false;
        }

        public abstract GameMode GetMode { get; }

        protected void IssueServerResponse(PackageType responseType)
        {
            WaitingForResponse = true;
            ParentControl.IssueServerResponse(new SubControlResponseRequest(GetMode, responseType, 5000));
        }

        public void ProcessServerResponse(PackageInterface responsePackage)
        {
            WaitingForResponse = false;
            ServerResponseActions(responsePackage);
        }

        public void HandleResponseTimeout(PackageType requestedPackageType)
        {
            WaitingForResponse = false;
            ResponseTimeoutActions(requestedPackageType);
        }

        protected virtual void ServerResponseActions(PackageInterface responsePackage) { }
        protected virtual void ResponseTimeoutActions(PackageType requestedPackageType) { }

        public abstract void HandleInput();
        
        public abstract void Update(GameTime gameTime);

        public void Draw(GameTime gameTime)
        {
            Visualizer.Draw(gameTime);
        }

        public void InitializeVisualizer(XNAInitializationData initData)
        {
            Visualizer.Initialize(initData);
        }

        protected void Log(string text)
        {
            ParentControl.Log(text);
        }

    }
}
