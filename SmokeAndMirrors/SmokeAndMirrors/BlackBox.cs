namespace SmokeAndMirrors
{
    public class BlackBox
    {
        private Tile[,] _tiles;

        public int Width => _tiles.GetLength(0);
        public int Height => _tiles.GetLength(1);

        public BlackBox(Tile[,] tiles)
        {
            _tiles = tiles;
        }

        public Tile this[int x, int y]
        {
            get { return _tiles[x, y]; }
            set { _tiles[x, y] = value; }
        }

        public Tile this[Vector v]
        {
            get { return this[v.X, v.Y]; }
            set { this[v.X, v.Y] = value; }
        }

        public EdgePosition Test(EdgePosition start)
        {
            Vector position;
            Vector direction;

            switch (start.Edge)
            {
                case Edge.Bottom:
                    position = new Vector(start.Offset, this.Height - 1);
                    direction = new Vector(0, -1);
                    break;
                case Edge.Top:
                    position = new Vector(start.Offset, 0);
                    direction = new Vector(0, 1);
                    break;
                case Edge.Left:
                    position = new Vector(0, start.Offset);
                    direction = new Vector(1, 0);
                    break;
                case Edge.Right:
                    position = new Vector(this.Width - 1, start.Offset);
                    direction = new Vector(-1, 0);
                    break;
                default:
                    return new EdgePosition();
            }

            while(true)
            {
                var t = this[position];
                switch (t)
                {
                    case Tile.Smoke:
                        return new EdgePosition();

                    case Tile.UpperLeftToLowerRight:
                        if (direction.Y == 0)
                            direction = new Vector(0, direction.X);
                        else
                            direction = new Vector(direction.Y, 0);
                        break;

                    case Tile.LowerLeftToUpperRight:
                        if (direction.Y == 0)
                            direction = new Vector(0, -direction.X);
                        else
                            direction = new Vector(-direction.Y, 0);
                        break;
                }

                position = position + direction;

                if (position.X < 0)
                    return new EdgePosition(Edge.Left, position.Y);
                if (position.X >= this.Width)
                    return new EdgePosition(Edge.Right, position.Y);
                if (position.Y < 0)
                    return new EdgePosition(Edge.Top, position.X);
                if (position.Y >= this.Height)
                    return new EdgePosition(Edge.Bottom, position.X);
            }
        }
    }
}
