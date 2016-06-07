using GameLogicLibrary.GameObjects;
using Microsoft.Xna.Framework;
using NetworkLibrary.DataStructs;

namespace PingPongClient.ControlLayer
{
    class Interpolation
    {
        GameStructure m_structure;

        public Interpolation(GameStructure structure)
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
