using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GameLogicLibrary
{
    public static class GameMath
    {
        public static float DegreeToRadian(float angle)
        {
            return (float)Math.PI * angle / 180.0F;
        }
    }
}
