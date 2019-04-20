using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace BallSimulator
{
    class BallSprite : UIElement
    {
        public readonly Brush Brush;
        public readonly Ball Ball;
        public readonly Canvas Surface;

        public BallSprite(Brush brush, Canvas surface, double radius, double mass, double x, double y, double vx, double vy)
        {
            this.Brush = brush;
            this.Surface = surface;
            this.Ball = new Ball(radius, mass, x, y, vx, vy);

            surface.Children.Add(this);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            drawingContext.DrawEllipse(this.Brush, null, this.Location, this.Radius, this.Radius);
        }

        public void Destroy()
        {
            this.Surface.Children.Remove(this);
        }

        public Point Location
        {
            get
            {
                double scale = this.Surface.ActualHeight / 100.0;

                return new Point(this.Ball.X * scale, this.Surface.ActualHeight - this.Ball.Y * scale);
            }
        }

        public double Radius
        {
            get
            {
                double scale = this.Surface.ActualHeight / 100.0;

                return this.Ball.Radius * scale;
            }
        }
    }
}
