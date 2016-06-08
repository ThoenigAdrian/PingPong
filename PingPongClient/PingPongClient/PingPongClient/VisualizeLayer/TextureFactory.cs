using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PingPongClient.VisualizeLayer
{
    class TextureFactory
    {
        public static Texture2D CreateRectangeTexture(GraphicsDevice graphics)
        {
            Texture2D rect = new Texture2D(graphics, 1, 1);
            rect.SetData(new Color[] { Color.White });
            return rect;
        }

        public static Texture2D CreateCircleTexture(int radius, GraphicsDevice graphics)
        {
            Texture2D texture = new Texture2D(graphics, radius, radius);
            Color[] colorData = new Color[radius * radius];

            float diam = radius / 2f;
            float diamsq = diam * diam;

            for (int x = 0; x < radius; x++)
            {
                for (int y = 0; y < radius; y++)
                {
                    int index = x * radius + y;
                    Vector2 pos = new Vector2(x - diam, y - diam);
                    if (pos.LengthSquared() <= diamsq)
                    {
                        colorData[index] = Color.White;
                    }
                    else
                    {
                        colorData[index] = Color.Transparent;
                    }
                }
            }

            texture.SetData(colorData);
            return texture;
        }
    }
}
