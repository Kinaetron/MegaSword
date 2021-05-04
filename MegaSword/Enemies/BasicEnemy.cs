using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PolyOne;
using PolyOne.Engine;
using PolyOne.Collision;
using PolyOne.Scenes;
using PolyOne.Components;

namespace MegaSword.Enemies
{
    public class BasicEnemy : Entity
    {
        private Texture2D texture;
        private Color colour;
        private CounterSet<string> counters = new CounterSet<string>();

        public float HitPoints { get; set; } = 30;

        private bool dead;

        public BasicEnemy(Vector2 position)
            :base(position)
        {
            this.Tag((int)GameTags.Enemy);
            this.Collider = new Hitbox((float)16.0f, (float)32.0f);
            this.Visible = true;

            texture = Engine.Instance.Content.Load<Texture2D>("Player");

            this.Add(counters);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
        }

        public override void Update()
        {
            base.Update();

            if(dead == false && counters["bulletTimer"] <= 0)
            {
                counters["bulletTimer"] = 3000;
                Bullet bullet = new Bullet(new Vector2(Position.X + 20, Position.Y + 8));
                this.Scene.Add(bullet);
                bullet.Added(this.Scene);
            }

            if(dead == false) {
                HitCollision();
            }


            if(HitPoints <= 0 && dead == false) {
                dead = true;
                counters["deadTimer"] = 2000.0f;
            }
            
            if(HitPoints <= 0 && dead == true && counters["deadTimer"] <= 0) {
                HitPoints = 30;
                dead = false;
            }
        }

        private void HitCollision()
        {
            if (counters["hitTimer"] > 0) {
                colour = Color.Red;
            }
            else {
                colour = Color.White;
            }

            using (List<Entity>.Enumerator enumerator = base.Scene[(int)GameTags.Player].GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    Player player = (Player)enumerator.Current;
                    foreach (Hitbox hurt in player.Hurtboxes)
                    {
                        if(counters["hitTimer"] > 0) {
                            return;
                        }

                        if (hurt.Intersects((Hitbox)Collider) == true &&
                            player.Action != PlayerAction.Parry)
                        {
                            HitPoints -= player.HurtPoints;
                            counters["hitTimer"] = 150.0f;
                            player.HitEnemy();
                        }
                    }
                }
            }
        }

        public void Hit()
        {
            if (counters["hitTimer"] > 0) {
                return;
            }

            counters["hitTimer"] = 150.0f;
        }


        public override void Draw()
        {
            if(dead == false) {
                Engine.SpriteBatch.Draw(texture, Position, colour);
            }

            base.Draw();
        }
    }
}
