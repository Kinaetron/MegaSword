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
    public class MovingPlatform : Platform
    {

        public bool IsPlayerOn { get; private set; }

        public float Velocity { get; private set; }
        private Vector2 remainder;
        private Level level;

        private Texture2D platformTexture;

        public MovingPlatform(Vector2 position) :
            base(position)
        {
            Velocity = 1;
            platformTexture = Engine.Instance.Content.Load<Texture2D>("Tiles/MovingPlatform");
            this.Tag((int)GameTags.MovingPlatform);
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
            bool isInPlayer = base.CollideCheck((int)GameTags.Player, Position - Vector2.UnitY);

            if (isInPlayer == true)
            {
                if (level.Player.Bottom <= Top) {
                    IsPlayerOn = true;
                }
                else {
                    IsPlayerOn = false;
                }
            }
            else {
                IsPlayerOn = false;
            }

            MovementHorizontal(Velocity);
        }

        private void MovementHorizontal(float amount)
        {
            remainder.X += amount;
            int move = (int)Math.Round((double)remainder.X);

            if (move != 0)
            {
                remainder.X -= move;
                int sign = Math.Sign(move);

                while (move != 0)
                {
                    Vector2 newPosition = Position + new Vector2(sign, 0);

                    if (this.CollideFirstOutside((int)GameTags.StopPoint, newPosition) != null)
                    {
                        Velocity = -Velocity;
                        remainder.X = 0;
                        break;
                    }

                    if (this.CollideFirstOutside((int)GameTags.Solid, newPosition) != null)
                    {
                        Velocity = -Velocity;
                        remainder.X = 0;
                        break;
                    }

                    Position.X += sign;
                    move -= sign;
                }
            }
        }

        public override void Draw()
        {
            base.Draw();
            Engine.SpriteBatch.Draw(platformTexture, Position,  Color.White);
        }
    }
}
