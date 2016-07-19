using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogicLibrary
{
    public class GameMath
    {
        public double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }
    }
}
