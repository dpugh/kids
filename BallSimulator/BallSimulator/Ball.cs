using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace BallSimulator
{
    public class Ball : ICollidable
    {
        public readonly double Radius;
        public readonly double Mass;

        public double X { get; set; }
        public double Y { get; set; }

        public double Vx { get; set; }
        public double Vy { get; set; }

        public Ball(double radius, double mass, double x, double y, double vx, double vy)
        {
            this.Radius = radius;
            this.Mass = mass;
            this.X = x;
            this.Y = y;
            this.Vx = vx;
            this.Vy = vy;
        }

        public void Update(double dt)
        {
            this.X += this.Vx * dt;
            this.Y += this.Vy * dt + 0.5 * Globals.Gravity * dt * dt;

            this.Vy = this.Vy + Globals.Gravity * dt;
        }

        public double PotentialEnergy
        {
            get
            {
                return -1.0 * this.Mass * Globals.Gravity * (this.Y - this.Radius);
            }
        }

        public double KineticEnergy
        {
            get
            {
                return 0.5 * this.Mass * (this.Vx * this.Vx + this.Vy * this.Vy);
            }
        }

        // Solve for the first collision (if any) between this and other.
        public double TimeOfCollision(Ball other)
        {
            // Solve:
            //  DeltaX(T) = this.X + this.Vx * T - (other.X + other.Vx * T) = (this.X - other.X) + (this.Vx - other.Vx) * T
            //  DeltaY(T) = this.Y + this.Vy * T - (other.Y + other.Vy * T) = (this.Y - other.Y) + (this.Vy - other.Vy) * T
            //  Distance(T)^2 = DeltaX(T)^2 + DeltaY(T)^2 = (this.Radius + other.Radius)^2
            //
            //  dx = (this.X - other.X)     dvx = (this.Vx - other.Vx)
            //  dy = (this.Y - other.Y)     dvy = (this.Vy - other.Vy)
            //
            //  r = this.Radius + other.Radius

            double dx = this.X - other.X;
            double dy = this.Y - other.Y;

            double dvx = this.Vx - other.Vx;
            double dvy = this.Vy - other.Vy;

            double r = this.Radius + other.Radius;

            //
            //  Distance(T)^2 = dx * dx + 2 * dx * dvx * T + dvx * dvx * T * T +
            //                  dy * dy + 2 * dy * dvy * T + dvy * dvy * T * T  = r*r
            //
            //  0 = (dx * dx + dy * dy) - r*r +
            //      2 * (dx * dvx + dy * dvx) * T +
            //      (dvx * dvx + dvy * dvy) * T * t
            //
            //  0 = c + b * T + a * T * T;

            double d = dx * dx + dy * dy;
            if (d == 0.0)
            {
                // The balls are exactly on top of one-another. This should never happen
                // but ignore the collision when it does since we can't handle it in
                // ResolveCollision();
                return -1.0;
            }

            double c = d - r * r;
            double b = 2.0 * (dx * dvx + dy * dvy);
            double a = (dvx * dvx + dvy * dvy);

            // Quadratic equation T = (-b +- sqrt(b*b - 4ac)) / (2*a)
            if (b >= 0.0)
            {
                // The objects are moving away from each other so there is no possibility of a collision.
                return -1.0;
            }

            if (c <= 0.0)
            {
                // The objects are overlapping, which shouldn't happen (probably due to round off error or initial placement)
                // Treat this as a collision right now.
                return 0.0;
            }

            double root = b*b - 4.0 * a * c;
            if (root < 0.0)
            {
                // The objects will never pass close enough to collide.
                return -1.0;
            }

            // We're interested in the time of the 1st collision and, since we know b < 0, sqrt(root) > 0
            // that corresponds to (-b - sqrt(root)) / (2.0 * a) => (b + sqrt(root)) / (-2.0 * a)
            return (b + Math.Sqrt(root)) / (-2.0 * a);
        }

        // Adjust the velocities of this and other as if they collided (
        public void ResolveCollision(Ball other)
        {
            // The collision only affects the component of the velocities along the vector between
            // the two objects so find the vector.
            double dx = other.X - this.X;
            double dy = other.Y - this.Y;
            double length = Math.Sqrt(dx * dx + dy * dy);
            if (length == 0.0)
            {
                // The balls are exactly on top of one-another. This should never happen
                // (and it should never be reported as a collision) so simply ignore it.
                return;
            }

            // Normalize dx, dy to a vector with a length of 1.0 to simplify the math later on.
            dx = dx / length;
            dy = dy / length;

            // Get a vector that is perpendicular to (dx, dy)
            double px = -dy;
            double py = dx;

            // Get the velocities of the objects directly towards one another and the velocities
            // perpendicular to each other.
            //
            // v1 is the velocity of this towards other. v1p is the perpendicular component.
            // v2 is the velocity of other towards this. v2p is the perpendicular component.
            double v1 = (this.Vx * dx) + (this.Vy * dy);
            double v1p = (this.Vx * px) + (this.Vy * py);

            double v2 = (other.Vx * dx) + (other.Vy * dy);
            double v2p = (other.Vx * px) + (other.Vy * py);

            if ((v1 - v2) <= 0.0)
            {
                // The objects are not moving towards one another, so we should skip resolving the collision.
                return;
            }

            double combinedMass = this.Mass + other.Mass;

            double newV1 = ((other.Mass * (v2 + Globals.CoefficientOFRestitutionBalls * (v2 - v1))) +
                             (this.Mass * v1)) / combinedMass;
            double newV2 = ((this.Mass * (v1 + Globals.CoefficientOFRestitutionBalls * (v1 - v2))) +
                             (other.Mass * v2)) / combinedMass;


            //double newV1 = ((2.0 * other.Mass * v2) + (this.Mass - other.Mass) * v1) / combinedMass;
            //double newV2 = ((2.0 * this.Mass * v1) + (other.Mass - this.Mass) * v2) / combinedMass;

            this.SetVelocity(newV1 * dx + v1p * px, newV1 * dy + v1p * py);
            other.SetVelocity(newV2 * dx + v2p * px, newV2 * dy + v2p * py);
        }

        internal void SetVelocity(double vx, double vy)
        {
            this.Vx = vx;
            this.Vy = vy;
        }
    }
}
