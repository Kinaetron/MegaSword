using Microsoft.Xna.Framework;

using PolyOne.Scenes;
using PolyOne.Engine;
using PolyOne.Utility;

using MegaSword.Platforms;
using MegaSword.Enemies;

using PolyOne.LevelProcessor;

namespace MegaSword
{
    public enum GameTags
    {
        None = 0,
        Player = 1,
        Solid = 2,
        Oneway = 3,
        FallingPlatform = 4,
        MovingPlatform = 5,
        MovingPlatformVert = 6,
        StopPoint = 7,
        Bullet = 8,
        Enemy = 9
    }

    public class Level : Scene
    {
       public Player Player { get; set; }

        LevelTilesSolid tiles;
        LevelTilesStopPoints stopPoints;

        public LevelTiler Tile { get; private set; }
        LevelData levelData = new LevelData();

        bool[,] collisionInfo;
        bool[,] onewayInfo;
        bool[,] stopPointsInfo;

        public override void LoadContent()
        {
            base.LoadContent();

            Tile = new LevelTiler();

            levelData = Engine.Instance.Content.Load<LevelData>("TestLevel");
            Tile.LoadContent(levelData);

            collisionInfo = LevelTiler.TileConverison(Tile.CollisionLayer, 2);
            tiles = new LevelTilesSolid(collisionInfo);
            this.Add(tiles);

            stopPointsInfo = LevelTiler.TileConverison(Tile.CollisionLayer, 4);
            stopPoints = new LevelTilesStopPoints(stopPointsInfo);
            this.Add(stopPoints);

            Player = new Player(Tile.PlayerPosition[0]);
            this.Add(Player);
            Player.Added(this);

            onewayInfo = LevelTiler.TileConverison(Tile.CollisionLayer, 3);

            for (int i = 0; i < onewayInfo.GetLength(0); i++) {
                for (int j = 0; j < onewayInfo.GetLength(1); j++)
                {
                    bool tile = onewayInfo[i,j];
                    if(tile == false) {
                        continue;
                    }

                    Vector2 position = new Vector2(i * TileInformation.TileWidth, j * TileInformation.TileHeight);
                    Oneway platform = new Oneway(position);
                    this.Add(platform);
                    platform.Added(this);
                }
            }

            foreach (Entity entity in Tile.Entites)
            {
                if (entity.Type == "Enemy")
                {
                    BasicEnemy enemy = new BasicEnemy(entity.Position);
                    this.Add(enemy);
                    enemy.Added(this);
                }

                if (entity.Type == "FallingPlatform")
                {
                    FallingPlatform fallingPlatform = new FallingPlatform(entity.Position);
                    this.Add(fallingPlatform);
                    fallingPlatform.Added(this);
                }

                if(entity.Type == "MovingPlatform")
                {
                    MovingPlatform movingPlatform = new MovingPlatform(entity.Position);
                    this.Add(movingPlatform);
                    movingPlatform.Added(this);
                }

                if (entity.Type == "MovingPlatformVert")
                {
                    MovingPlatformVert movingPlatform = new MovingPlatformVert(entity.Position);
                    this.Add(movingPlatform);
                    movingPlatform.Added(this);
                }
            }
        }

        public override void UnloadContent()
        {
            base.UnloadContent();
        }

        public override void Update()
        {
            base.Update();
        }

        public override void Draw()
        {
            Engine.BeginParallax(Player.Camera.TransformMatrix);
            Tile.DrawImageBackground(Player.Camera.Position);
            Engine.End();

            Engine.Begin(Player.Camera.TransformMatrix);
            Tile.DrawBackground();
            base.Draw();
            Engine.End();
        }
    }
}
