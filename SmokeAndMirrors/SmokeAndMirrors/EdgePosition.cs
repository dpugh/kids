namespace SmokeAndMirrors
{
    public struct EdgePosition
    {
        public readonly Edge Edge;
        public readonly int Offset;

        public EdgePosition(Edge edge = Edge.None, int offset = 0)
        {
            this.Edge = edge;
            this.Offset = offset;
        }

        public override int GetHashCode()
        {
            return this.Edge == Edge.None
                   ? -1
                   : ((this.Offset << 2) | (int)(this.Edge));
        }

        public override bool Equals(object obj)
        {
            if (obj is EdgePosition position)
            {
                return (this.Edge == position.Edge) &&
                       ((this.Edge == Edge.None) || (this.Offset == position.Offset));
            }

            return false;
        }

        public override string ToString()
        {
            return "(" + this.Edge.ToString() + ":" + this.Offset.ToString() + ")";
        }
    }
}
