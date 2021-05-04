using Microsoft.Xna.Framework;

using PolyOne;

namespace MegaSword.Platforms
{
    public abstract class Platform : Entity
    {
        public Platform(Vector2 position)
            : base(position)
        {
        }
    }
}
