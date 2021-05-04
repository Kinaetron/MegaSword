using MegaSword.Enemies;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;


using PolyOne;
using PolyOne.Collision;
using PolyOne.Components;
using PolyOne.Engine;
using PolyOne.Scenes;
using System.Collections.Generic;

namespace MegaSword
{
    public enum BulletDirection
    {
        None = 0,
        Right = 1,
        Left = 2
    }

    public class PlayerBullet : Entity
    {
        private Texture2D texture;
        private float speed = 10.0f;

        private const int hurtPoints = 1;

        CounterSet<string> counters = new CounterSet<string>();
        private const float pauseTime = 66.8f;

        private BulletDirection direction = BulletDirection.None;


        public PlayerBullet(Vector2 position, BulletDirection direction)
           : base(position)
        {
            texture = Engine.Instance.Content.Load<Texture2D>("Bullet");

            this.Tag((int)GameTags.Bullet);
            this.Collider = new Hitbox(10, 10);

            this.Visible = true;

            this.direction = direction;
        }

        public override void Added(Scene scene) {
            base.Added(scene);
        }

        public override void Update()
        {
            if(direction == BulletDirection.Right) {
                Position.X += speed;
            }
            else if(direction == BulletDirection.Left) {
                Position.X -= speed;
            }

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

            if (this.CollideFirst((int)GameTags.Enemy, Position) != null) {
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

            base.Update();
        }

        public override void Draw()
        {
            Engine.SpriteBatch.Draw(texture, Position, Color.White);
            base.Draw();
        }
    }
}
