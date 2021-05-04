using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using PolyOne;
using PolyOne.Collision;
using PolyOne.Components;
using PolyOne.Engine;
using PolyOne.Scenes;

namespace MegaSword.Platforms
{
    public class FallingPlatform : Platform
    {
        private Level level;
        private bool isPlayerOn;

        private Vector2 originalPosition;

        private Vector2 velocity;

        private Texture2D platformTexture;
        CounterSet<string> counters = new CounterSet<string>();

        Color color = Color.White;

        private bool timerSet = false;
        private bool respawnSet = false;
        private const float fallTime = 500.0f;
        private const float respawnTime = 3000.0f;

        public FallingPlatform(Vector2 position) :
            base(position)
        {
            originalPosition = position;
            platformTexture = Engine.Instance.Content.Load<Texture2D>("Tiles/FallingPlatform");
            this.Tag((int)GameTags.FallingPlatform);
            this.Collider = new Hitbox((float)48.0f, (float)16.0f, 0.0f, 0.0f);
            this.Visible = true;
            this.Active = true;

            this.Add(counters);
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

            isPlayerOn = base.CollideCheck((int)GameTags.Player, this.Position - Vector2.UnitY);

            if(isPlayerOn == true && timerSet == false) {
                timerSet = true;
                counters["fallTimer"] = fallTime;
            }

            if(counters["fallTimer"] <= 500  && timerSet == true) {
                color = Color.Red;
            }

            if(counters["fallTimer"] <= 0 && timerSet == true) {
                velocity.Y += 2.0f;
                velocity.Y = MathHelper.Clamp(velocity.Y, -6, 6);
            }

            Position.Y += velocity.Y;

            if (counters["fallTimer"] <= 0 && respawnSet == false && timerSet == true) {
                counters["respawnTimer"] = respawnTime;
                respawnSet = true;
            }

            if(counters["respawnTimer"] <= 0 && respawnSet == true)
            {
                velocity.Y = 0.0f;
                timerSet = false;
                respawnSet = false;
                color = Color.White;
                Position = originalPosition;
            }
        }

        public override void Draw()
        {
            base.Draw();
            Engine.SpriteBatch.Draw(platformTexture, Position, color);
        }
    }
}
