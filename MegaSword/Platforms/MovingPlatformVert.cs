using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PolyOne;
using PolyOne.Collision;
using PolyOne.Components;
using PolyOne.Engine;
using PolyOne.Scenes;

namespace MegaSword.Platforms
{
    public class MovingPlatformVert : Platform
    {
        public bool IsPlayerOn { get; private set; }

        public float Velocity { get; private set; }
        private Vector2 remainder;
        private Level level;

        private Texture2D platformTexture;

        public MovingPlatformVert(Vector2 position) :
            base(position)
        {
            Velocity = 1;
            platformTexture = Engine.Instance.Content.Load<Texture2D>("Tiles/MovingPlatform");
            this.Tag((int)GameTags.MovingPlatformVert);
            this.Collider = new Hitbox((float)32.0f, (float)32.0f, 0.0f, 0.0f);
            this.Visible = true;
            this.Active = true;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);

            if (base.Scene is Level) {
                this.level = (base.Scene as Level);
            }
        }

        public override void Update()
        {
            base.Update();
            IsPlayerOn = base.CollideCheck((int)GameTags.Player, Position - Vector2.UnitY);
            MovementVerical(Velocity);
        }

        private void MovementVerical(float amount)
        {
            remainder.Y += amount;
            int move = (int)Math.Round((double)remainder.Y);

            if (move < 0)
            {
                remainder.Y -= move;
                while (move != 0)
                {
                    Vector2 newPosition = Position + new Vector2(0, -1.0f);
                    if (this.CollideFirstOutside((int)GameTags.StopPoint, newPosition) != null)
                    {
                        Velocity = -Velocity;
                        remainder.Y = 0;
                        break;
                    }

                    if (this.CollideFirstOutside((int)GameTags.Solid, newPosition) != null)
                    {
                        Velocity = -Velocity;
                        remainder.Y = 0;
                        break;
                    }
                    Position.Y += -1.0f;
                    move -= -1;
                }
            }
            else if (move > 0)
            {
                remainder.Y -= move;
                while (move != 0)
                {
                    Vector2 newPosition = Position + new Vector2(0, 1.0f);
                    if (this.CollideFirstOutside((int)GameTags.StopPoint, newPosition) != null)
                    {
                        Velocity = -Velocity;
                        remainder.Y = 0;
                        break;
                    }

                    if (this.CollideFirstOutside((int)GameTags.Solid, newPosition) != null)
                    {
                        Velocity = -Velocity;
                        remainder.Y = 0;
                        break;
                    }
                    Position.Y += 1.0f;
                    move -= 1;
                }
            }
        }

        public override void Draw()
        {
            base.Draw();
            Engine.SpriteBatch.Draw(platformTexture, Position, Color.White);
        }
    }
}
