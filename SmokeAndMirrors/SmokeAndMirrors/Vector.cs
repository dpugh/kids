namespace SmokeAndMirrors
{
    public struct Vector
    {
        public readonly int X;
        public readonly int Y;
        public Vector(int x, int y) { this.X = x; this.Y = y; }

        public static Vector operator +(Vector left, Vector right)
        {
            return new Vector(left.X + right.X, left.Y + right.Y);
        }

        public override string ToString()
        {
            return "(" + this.X.ToString() + ", " + this.Y.ToString() + ")";
        }
    }
}
