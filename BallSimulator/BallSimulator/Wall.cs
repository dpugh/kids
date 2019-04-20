using System;
using System.Windows;

namespace BallSimulator
{
    public class HorizontalWall : ICollidable
    {
        public readonly double Y;

        public HorizontalWall(double y)
        {
            this.Y = y;
        }

        public double TimeOfCollision(Ball other)
        {
            if ((other.Vy == 0.0) || (other.Y == this.Y))
            {
                // Either the ball is directly on top of us or it isn't moving horizontally. Either way, no collision.
                return -1.0;
            }
            else if (this.Y > other.Y)
            {
                // We are the right of the ball.
                if (other.Vy <= 0.0)
                {
                    // The ball is moving to the left, no collision.
                    return -1.0;
                }

                // Find the time of the collision, reporting now if the ball is too close.
                return Math.Max(0.0, (this.Y - (other.Y + other.Radius)) / other.Vy);
            }
            else
            {
                // We are the left of the ball.
                if (other.Vy >= 0.0)
                {
                    // The ball is moving to the right, no collision.
                    return -1.0;
                }

                // Find the time of the collision, reporting now if the ball is too close.
                return Math.Max(0.0, (this.Y - (other.Y - other.Radius)) / other.Vy);
            }
        }

        public void ResolveCollision(Ball other)
        {
            if (this.Y > other.Y)
            {
                if (other.Vy > 0.0)
                {
                    other.SetVelocity(other.Vx, other.Vy * -1.0 * Globals.CoefficientOFRestitutionTopAndBottom);
                }
            }
            else
            {
                // We're to the left of the ball. If the ball is moving to the left, bounce.
                if (other.Vy < 0.0)
                {
                    other.SetVelocity(other.Vx, other.Vy * -1.0 * Globals.CoefficientOFRestitutionTopAndBottom);
                }
            }
        }
    }

    public class VerticalWall : ICollidable
    {
        public readonly double X;

        public VerticalWall(double x)
        {
            this.X = x;
        }

        public double TimeOfCollision(Ball other)
        {
            if ((other.Vx == 0.0) || (other.X == this.X))
            {
                // Either the ball is directly on top of us or it isn't moving horizontally. Either way, no collision.
                return -1.0;
            }
            else if (this.X > other.X)
            {
                // We are the right of the ball.
                if (other.Vx <= 0.0)
                {
                    // The ball is moving to the left, no collision.
                    return -1.0;
                }

                // Find the time of the collision, reporting now if the ball is too close.
                return Math.Max(0.0, (this.X - (other.X + other.Radius)) / other.Vx);
            }
            else
            {
                // We are the left of the ball.
                if (other.Vx >= 0.0)
                {
                    // The ball is moving to the right, no collision.
                    return -1.0;
                }

                // Find the time of the collision, reporting now if the ball is too close.
                return Math.Max(0.0, (this.X - (other.X - other.Radius)) / other.Vx);
            }
        }

        public void ResolveCollision(Ball other)
        {
            if (this.X > other.X)
            {
                if (other.Vx > 0.0)
                {
                    other.SetVelocity(other.Vx * -1.0 * Globals.CoefficientOFRestitutionLeftAndRight, other.Vy);
                }
            }
            else
            {
                // We're to the left of the ball. If the ball is moving to the left, bounce.
                if (other.Vx < 0.0)
                {
                    other.SetVelocity(other.Vx * -1.0 * Globals.CoefficientOFRestitutionLeftAndRight, other.Vy);
                }
            }
        }
    }
}
