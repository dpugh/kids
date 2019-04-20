using System;
using BallSimulator;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTestProject1
{
    [TestClass]
    public class BallTests
    {
        [TestMethod]
        public void BallTimeOfCollisionTests()
        {
            Ball b1 = new Ball(1.0, 1.0, 0.0, 0.0, 1.0, 0.0);
            Ball b2 = new Ball(1.0, 1.0, 3.0, 0.0, 0.0, 0.0);
            VerifyTimeOfCollision(b1, b2, true);

            Ball b3 = new Ball(1.0, 1.0, 5.0, -5.0, 0.0, 1.0);
            VerifyTimeOfCollision(b1, b3, true);

            Ball b4 = new Ball(1.0, 1.0, 0.0, 5.0, 0.0, 1.0);
            VerifyTimeOfCollision(b1, b4, false);

            Ball b5 = new Ball(1.0, 1.0, 5.0, 1.0, 0.0, 1.0);
            VerifyTimeOfCollision(b1, b5, false);
        }

        [TestMethod]
        public void HorizontalTimeOfCollisionTests()
        {
            Ball b1 = new Ball(1.0, 1.0, 5.0, 5.0, 1.0, 0.0);
            Ball b2 = new Ball(1.0, 1.0, 5.0, 5.0, -10.0, 0.0);
            HorizontalWall h1 = new HorizontalWall(10.0);
            HorizontalWall h2 = new HorizontalWall(-10.0);

            VerifyTimeOfCollision(b1, h1, true);
            VerifyTimeOfCollision(b1, h2, false);
            VerifyTimeOfCollision(b2, h1, false);
            VerifyTimeOfCollision(b2, h2, true);
        }

        [TestMethod]
        public void VerticalTimeOfCollisionTests()
        {
            Ball b1 = new Ball(1.0, 1.0, 5.0, 5.0, 1.0, 20.0);
            Ball b2 = new Ball(1.0, 1.0, 5.0, 5.0, 7, -10.0);
            VerticalWall v1 = new VerticalWall(10.0);
            VerticalWall v2 = new VerticalWall(-10.0);

            VerifyTimeOfCollision(b1, v1, true);
            VerifyTimeOfCollision(b1, v2, true);
            VerifyTimeOfCollision(b2, v1, false);
            VerifyTimeOfCollision(b2, v2, true);
        }

        [TestMethod]
        public void ResolveCollisionTests()
        {
            Ball b1 = new Ball(1.0, 1.0, 0.0, 0.0, 1.0, 1.0);
            Ball b2 = new Ball(1.0, 1.0, 3.0, 0.0, 0.0, 1.0);

            VerifyResultsOfCollision(b1, b2);

            Ball b3 = new Ball(1.0, 2.0, 0.0, 0.0, 1.0, 1.0);
            Ball b4 = new Ball(1.0, 1.0, 3.0, 0.0, 0.0, 1.0);

            VerifyResultsOfCollision(b2, b4);
        }

        public void VerifyTimeOfCollision(Ball b1, Ball b2, bool expectedToCollide)
        {
            double t1 = b1.TimeOfCollision(b2);
            double t2 = b2.TimeOfCollision(b1);
            Assert.AreEqual(t1, t2, 0.01);

            if (t1 > 0.0)
            {
                Assert.IsTrue(expectedToCollide);
                double x1 = b1.X + b1.Vx * t1;
                double y1 = b1.Y + b1.Vy * t1 + 0.5 * Globals.Gravity * t1 * t1;

                double x2 = b2.X + b2.Vx * t1;
                double y2 = b2.Y + b2.Vy * t1 + 0.5 * Globals.Gravity * t1 * t1;

                double dx = x1 - x2;
                double dy = y1 - y2;

                double d = Math.Sqrt(dx * dx + dy * dy);
                Assert.AreEqual(b1.Radius + b2.Radius, d, 0.01);
            }
            else
            {
                Assert.IsFalse(expectedToCollide);
            }
        }

        public void VerifyTimeOfCollision(Ball b1, VerticalWall v, bool expectedToCollide)
        {
            double t1 = v.TimeOfCollision(b1);

            if (t1 > 0.0)
            {
                Assert.IsTrue(expectedToCollide);
                double x1 = b1.X + b1.Vx * t1;

                if (v.X > b1.X)
                {
                    Assert.AreEqual(v.X - b1.Radius, x1, 0.01);
                }
                else
                {
                    Assert.AreEqual(v.X + b1.Radius, x1, 0.01);
                }
            }
            else
            {
                Assert.IsFalse(expectedToCollide);
            }

        }

        public void VerifyTimeOfCollision(Ball b1, HorizontalWall h, bool expectedToCollide)
        {
            double t1 = h.TimeOfCollision(b1);

            if (t1 > 0.0)
            {
                Assert.IsTrue(expectedToCollide);
                double y1 = b1.Y + b1.Vy * t1 + 0.5 * Globals.Gravity * t1 * t1;

                if (h.Y > b1.Y)
                {
                    Assert.AreEqual(h.Y - b1.Radius, y1, 0.01);
                }
                else
                {
                    Assert.AreEqual(h.Y + b1.Radius, y1, 0.01);
                }
            }
            else
            {
                Assert.IsFalse(expectedToCollide);
            }

        }

        public void VerifyResultsOfCollision(Ball b1, Ball b2)
        {
            double v1Before = Math.Sqrt(b1.Vx * b1.Vx + b1.Vy * b1.Vy);
            double v2Before = Math.Sqrt(b2.Vx * b2.Vx + b2.Vy * b2.Vy);

            double momentumBeforeX = b1.Vx * b1.Mass + b2.Vx * b2.Mass;
            double momentumBeforeY = b1.Vy * b1.Mass + b2.Vy * b2.Mass;
            double energyBefore = v1Before * v1Before * b1.Mass + v2Before * v2Before * b2.Mass;

            b1.ResolveCollision(b2);

            double v1After = Math.Sqrt(b1.Vx * b1.Vx + b1.Vy * b1.Vy);
            double v2After = Math.Sqrt(b2.Vx * b2.Vx + b2.Vy * b2.Vy);

            double momentumAfterX = b1.Vx * b1.Mass + b2.Vx * b2.Mass;
            double momentumAfterY = b1.Vy * b1.Mass + b2.Vy * b2.Mass;
            double energyAfter = v1After * v1After * b1.Mass + v2After * v2After * b2.Mass;

            Assert.AreEqual(momentumBeforeX, momentumAfterX, 0.01);
            Assert.AreEqual(momentumBeforeY, momentumAfterY, 0.01);
            Assert.AreEqual(energyBefore, energyAfter, 0.01);
        }
    }
}
