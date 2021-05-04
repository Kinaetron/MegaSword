using Microsoft.Xna.Framework;

using PolyOne.Collision;
using PolyOne.Scenes;

namespace MegaSword.Platforms
{
    public class Oneway : Platform
    {
        bool isPlayerOn;
        public bool IsOnJumpThrough { get; set; }
        private Level level;

        public Oneway(Vector2 position)
            : base(position)
        {
            this.Tag((int)GameTags.Oneway);
            this.Collider = new Hitbox((float)16.0f, (float)16.0f, 0.0f, 0.0f);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (base.Scene is Level) {
                this.level = (base.Scene as Level);
            }
        }

        public Oneway()
            : base(Vector2.Zero)
        {
            this.Tag((int)GameTags.Oneway);
        }

        public override void Update()
        {
            base.Update();

            isPlayerOn = base.CollideCheck((int)GameTags.Player, Position - Vector2.UnitY);

            if(isPlayerOn == true)
            {    
                if(level.Player.Bottom <= Top) {
                    IsOnJumpThrough = true;
                }
                else {
                    IsOnJumpThrough = false;
                }
            }
            else {
               IsOnJumpThrough = false;
            }
        }


    }
}
