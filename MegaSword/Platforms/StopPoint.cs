using Microsoft.Xna.Framework;
using PolyOne.Collision;

namespace MegaSword.Platforms
{
    public class StopPoint : Platform
    {
        public Vector2 ActualPosition
        {
            get
            {
                return this.Position;
            }
        }

        public StopPoint(Vector2 position, int width, int height)
            : base(position)
        {
            this.Tag((int)GameTags.StopPoint);
            this.Collider = new Hitbox((float)width, (float)height, 0.0f, 0.0f);
        }

        public StopPoint()
            : base(Vector2.Zero)
        {
            this.Tag((int)GameTags.StopPoint);
        }
    }
}
