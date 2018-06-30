using System.Collections.Generic;
using GameLogicLibrary.GameObjects;

namespace GameLogicLibrary
{
    public class CollisionDetector
    {
        private bool PointInRectangular(GameStructure.Point point, GameStructure.Rectangle rectangle)
        {
            bool betweenXLine = (rectangle.PositionX <= point.PositionX) && (point.PositionX <= rectangle.PositionX + rectangle.Width);
            bool betweenYLine = (rectangle.PositionY <= point.PositionY) && (point.PositionY <= rectangle.PositionY + rectangle.Height);
            return betweenXLine && betweenYLine;
        }

        public bool CircleInRect(GameStructure.Circle circle, GameStructure.Rectangle rectangle)
        {
            GameStructure.Point lefmost = new GameStructure.Point(circle.PositionX - circle.Radius, circle.PositionY);
            GameStructure.Point rightmost = new GameStructure.Point(circle.PositionX + circle.Radius, circle.PositionY);
            GameStructure.Point topmost = new GameStructure.Point(circle.PositionX, circle.PositionY - circle.Radius);
            GameStructure.Point bottommost = new GameStructure.Point(circle.PositionX, circle.PositionY + circle.Radius);

            List<GameStructure.Point> cornerPoints = new List<GameStructure.Point>();
            cornerPoints.Add(lefmost);
            cornerPoints.Add(rightmost);
            cornerPoints.Add(topmost);
            cornerPoints.Add(bottommost);

            foreach (GameStructure.Point point in cornerPoints)
            {
                if (PointInRectangular(point, rectangle))
                    return true;
            }

            return false;
        }
    }
}
