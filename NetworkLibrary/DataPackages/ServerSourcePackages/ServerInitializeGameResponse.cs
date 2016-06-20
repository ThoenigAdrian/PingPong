using GameLogicLibrary.GameObjects;

namespace NetworkLibrary.DataPackages.ServerSourcePackages
{
    public class ServerInitializeGameResponse : PackageInterface
    {
        public override PackageType PackageType { get { return PackageType.ServerPlayerIDResponse; } }

        public GameField m_field;

        public Ball m_ball;

        public Player[] m_players;
    }
}
