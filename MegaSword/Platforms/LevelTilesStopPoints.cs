using PolyOne.Collision;
using PolyOne.Utility;

namespace MegaSword.Platforms
{
    public class LevelTilesStopPoints : StopPoint
    {
        public Grid Grid { get; private set; }

        public LevelTilesStopPoints(bool[,] solidData)
        {
            this.Active = false;
            this.Collider = (this.Grid = new Grid(TileInformation.TileWidth, TileInformation.TileHeight, solidData));
        }
    }
}
