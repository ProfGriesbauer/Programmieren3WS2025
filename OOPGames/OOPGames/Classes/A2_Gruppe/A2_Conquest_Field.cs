using System.Collections.Generic;

namespace OOPGames
{
    public class Field
    {
        private readonly Tile[,] _tiles;

        public int Width => _tiles.GetLength(0);
        public int Height => _tiles.GetLength(1);

        public Field(int width, int height)
        {
            _tiles = new Tile[width, height];
            for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                _tiles[x, y] = new Tile(x, y);
        }

        public Tile GetTile(int x, int y) => _tiles[x, y];

        public IEnumerable<Tile> AllTiles()
        {
            for (int x = 0; x < Width; x++)
            for (int y = 0; y < Height; y++)
                yield return _tiles[x, y];
        }

        public IEnumerable<Tile> GetNeighbours4(Tile t)
        {
            int x = t.X;
            int y = t.Y;

            if (x > 0) yield return _tiles[x - 1, y];
            if (x < Width - 1) yield return _tiles[x + 1, y];
            if (y > 0) yield return _tiles[x, y - 1];
            if (y < Height - 1) yield return _tiles[x, y + 1];
        }
    }
}
