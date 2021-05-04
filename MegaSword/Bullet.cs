using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PolyOne;
using PolyOne.Collision;
using PolyOne.Engine;
using PolyOne.Scenes;
using MegaSword.Enemies;
using PolyOne.Components;

namespace MegaSword
{
    public class Bullet : Entity
    {
        private Texture2D texture;

        private const float speed = 10.0f;
        private float velocity = speed;

        private const int hurtPoints = 1;

        private CounterSet<string> counters = new CounterSet<string>();
        private const float pauseTime = 66.8f;
        private bool bounceBack = false;

        public Bullet(Vector2 position)
           : base(position)
        {
            texture = Engine.Instance.Content.Load<Texture2D>("Bullet");

            this.Tag((int)GameTags.Bullet);
            this.Collider = new Hitbox(10, 10);

            this.Visible = true;
            this.Add(counters);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
        }

        public override void Update()
        {
            Position.X += velocity;

            if (this.CollideFirst((int)GameTags.Solid, Position) != null) {
                RemoveSelf();
            }

            if (this.CollideFirst((int)GameTags.MovingPlatform, Position) != null) {
                RemoveSelf();
            }

            if (this.CollideFirst((int)GameTags.MovingPlatformVert, Position) != null) {
                RemoveSelf();
            }

            if (this.CollideFirst((int)GameTags.FallingPlatform, Position) != null) {
                RemoveSelf();
            }

            using (List<Entity>.Enumerator enumerator = base.Scene[(int)GameTags.Enemy].GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    BasicEnemy enemy = (BasicEnemy)enumerator.Current;

                    if (this.CollideFirst((int)GameTags.Enemy, Position) != null)
                    {
                        enemy.Hit();
                        enemy.HitPoints -= 6;
                        RemoveSelf();
                    }
                }
            }

            using (List<Entity>.Enumerator enumerator = base.Scene[(int)GameTags.Player].GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Player player = (Player)enumerator.Current;
                    foreach (Hitbox hurt in player.Hurtboxes)
                    {
                        if (hurt.Intersects((Hitbox)Collider) == true &&
                            player.Action == PlayerAction.Parry && bounceBack == false)
                        {
                            player.Pause(pauseTime);
                            counters["parryTimer"] = pauseTime;
                            bounceBack = true;
                            velocity = 0;
                            return;
                        }
                    }

                    if (this.CollideFirst((int)GameTags.Player, Position) != null && bounceBack == false) {
                        player.Hurt(hurtPoints);
                        RemoveSelf();
                    }
                }
            }
            base.Update();

            if (counters["parryTimer"] > 0) {
                return;
            }

            if (bounceBack == true) {
                velocity = -speed;
                bounceBack = false;
            }
        }

        public override void Draw()
        {
            Engine.SpriteBatch.Draw(texture, Position, Color.White);
            base.Draw();
        }

    }
}
