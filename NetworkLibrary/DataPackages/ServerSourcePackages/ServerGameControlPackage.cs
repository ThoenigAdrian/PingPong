using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GameLogicLibrary;

namespace NetworkLibrary.DataPackages.ServerSourcePackages
{
    public class ServerGameControlPackage : PackageInterface
    {
        public ServerControls Command;
        public Teams Winner;
        public GameScore Score = new GameScore();

        public override PackageType PackageType
        {
            get { return PackageType.ServerGameControl; }
        }

        public class GameScore
        {
            public int Team1 = 0;
            public int Team2 = 0;
        }
    }
}
