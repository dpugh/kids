using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace BallSimulator
{
    class EnergySprite : UIElement
    {
        public readonly Brush Brush;

        public double EnergyOffset;
        public readonly Grid Parent;
        public readonly Ball Ball;
        public readonly double MaxEnergy;

        public EnergySprite(Brush brush, Grid parent, Ball ball, double maxEnergy)
        {
            this.Brush = brush;
            this.Parent = parent;
            this.Ball = ball;
            this.MaxEnergy = maxEnergy;

            this.Parent.Children.Add(this);
        }


        public void Destroy()
        {
            this.Parent.Children.Remove(this);
        }


        protected override void OnRender(System.Windows.Media.DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            double y = this.Parent.ActualHeight;
            double scale = y / MaxEnergy;

            double y1 = this.EnergyOffset * scale;
            double y2 = y1 + this.Ball.PotentialEnergy * scale;
            double y3 = y2 + this.Ball.KineticEnergy * scale;

            if (y2 > y1)
                drawingContext.DrawRectangle(Brushes.Green, null, new Rect(5.0, y - y2, 5.0, y2 - y1));
            if (y3 > y2)
                drawingContext.DrawRectangle(Brushes.Yellow, null, new Rect(5.0, y - y3, 5.0, y3 - y2));
            if (y3 > y1)
                drawingContext.DrawRectangle(this.Brush, null, new Rect(15.0, y - y3, 5.0, y3 - y1));
        }
    }
}
