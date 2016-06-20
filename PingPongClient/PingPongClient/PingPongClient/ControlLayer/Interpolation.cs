using GameLogicLibrary.GameObjects;
using Microsoft.Xna.Framework;
using NetworkLibrary.DataPackages.ServerSourcePackages;

namespace PingPongClient.ControlLayer
{
    class Interpolation
    {
        BasicStructure m_structure;

        public Interpolation()
        {
        }

        public void SetStructure(BasicStructure structure)
        {
            m_structure = structure;
        }

        public void UpdateData(ServerDataPackage serverData)
        {

        }

        public void Interpolate(GameTime gameTime)
        {
        }
    }
}
