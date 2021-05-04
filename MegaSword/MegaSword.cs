using Microsoft.Xna.Framework;

using PolyOne.Engine;
using PolyOne.Utility;

using MegaSword.Screens;

namespace MegaSword
{
    public class MegaSword : Engine
    {
        static readonly string[] preloadAssets =
        {
            "MenuAssets/gradient",
        };

        public MegaSword()
            : base(640, 360, "Koloon", 2.0f, false)
        {
        }

        protected override void Initialize()
        {

            base.Initialize();

            TileInformation.TileDiemensions(16, 16);

            screenManager.AddScreen(new BackgroundScreen(), null);
            screenManager.AddScreen(new MainMenuScreen(), null);
        }

        protected override void LoadContent()
        {
            base.LoadContent();

            foreach (string asset in preloadAssets) {
                Engine.Instance.Content.Load<object>(asset);
            }
        }

        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);
        }
    }

    static class Program
    {
        static void Main(string[] args)
        {
            using (MegaSword game = new MegaSword())
            {
                game.Run();
            }
        }
    }
}
