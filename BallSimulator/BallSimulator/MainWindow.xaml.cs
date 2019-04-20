using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace BallSimulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public IReadOnlyList<HorizontalWall> HorizontalWalls = new HorizontalWall[2]
                                                            {
                                                                new HorizontalWall(  0.0),
                                                                new HorizontalWall(100.0),
                                                            };

        public IReadOnlyList<VerticalWall> VerticalWalls = new VerticalWall[2]
                                                            {
                                                                new VerticalWall(  0.0),
                                                                new VerticalWall(100.0),
                                                            };

        DispatcherTimer _timer;

        List<BallSprite> Balls = new List<BallSprite>();
        List<EnergySprite> Energy = new List<EnergySprite>();
        DateTime _lastTick;

        List<Tuple<Point, Vector>[]> _states = new List<Tuple<Point, Vector>[]>();

        public MainWindow()
        {
            InitializeComponent();

            this.Scenarios.ItemsSource = this.StartingScenarios;

            _timer = new DispatcherTimer(DispatcherPriority.Normal, this.Dispatcher);

            _timer.Interval = TimeSpan.FromMilliseconds(10.0);
            _timer.Tick += this.OnTick;

            this.ResetSystem();

            this.Content.SizeChanged += this.OnSizeChanged;
            this.Arena.SizeChanged += this.OnArenaSizeChanged;
        }

        void OnTick(object sender, EventArgs e)
        {
            DateTime now = DateTime.Now;

            TimeSpan dt = now - _lastTick;
            _lastTick = now;

            this.AdvanceClock(dt.TotalSeconds);
        }

        void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateSize();
        }

        void OnArenaSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.PostUpdate();
        }

        void UpdateSize()
        {
            double freeHeight = this.Content.ActualHeight - 40.0;
            double freeWidth = this.Content.ActualWidth - 240.0;

            double size = Math.Min(freeHeight, freeWidth);

            this.Arena.Width = size;
            this.Arena.Height = size;
        }

        void AdvanceClock(double dt, bool breakOnFirstCollision = false)
        {
            for (int i = 0; (i < 25); ++i)
            {
                double timeOfFirstCollision = double.MaxValue;
                ICollidable collidable = null;
                Ball ball = null;

                foreach (var h in this.HorizontalWalls)
                {
                    foreach (var b in this.Balls)
                    {
                        double t = h.TimeOfCollision(b.Ball);
                        if ((t >= 0.0) && (t < timeOfFirstCollision))
                        {
                            timeOfFirstCollision = t;
                            collidable = h;
                            ball = b.Ball;
                        }
                    }
                }

                foreach (var v in this.VerticalWalls)
                {
                    foreach (var b in this.Balls)
                    {
                        double t = v.TimeOfCollision(b.Ball);
                        if ((t >= 0.0) && (t < timeOfFirstCollision))
                        {
                            timeOfFirstCollision = t;
                            collidable = v;
                            ball = b.Ball;
                        }
                    }
                }

                foreach (var b1 in this.Balls)
                {
                    foreach (var b2 in this.Balls)
                    {
                        if (b1 != b2)
                        {
                            double t = b1.Ball.TimeOfCollision(b2.Ball);
                            if ((t >= 0.0) && (t < timeOfFirstCollision))
                            {
                                timeOfFirstCollision = t;
                                collidable = b1.Ball;
                                ball = b2.Ball;
                            }
                        }
                    }
                }

                if (timeOfFirstCollision >= dt)
                {
                    this.MoveBalls(dt);
                    break;
                }
                else
                {
                    this.MoveBalls(timeOfFirstCollision);
                    collidable.ResolveCollision(ball);

                    dt -= timeOfFirstCollision;

                    if (breakOnFirstCollision)
                    {
                        break;
                    }
                }
            }

            this.AdvanceStates();
            this.PostUpdate();
        }

        public void AdvanceStates()
        {
            var newState = new Tuple<Point, Vector>[this.Balls.Count];
            for (int i = 0; (i < this.Balls.Count); ++i)
            {
                newState[i] = Tuple.Create(new Point(this.Balls[i].Ball.X, this.Balls[i].Ball.Y), new Vector(this.Balls[i].Ball.Vx, this.Balls[i].Ball.Vy));
            }
            _states.Add(newState);

            this.Rewind.IsEnabled = (_states.Count >= 2);
        }

        public void ReverseTick()
        {
            if (_states.Count >= 2)
            {
                _states.RemoveAt(_states.Count - 1);
                var state = _states[_states.Count - 1];
                for (int i = 0; (i < this.Balls.Count); ++i)
                {
                    this.Balls[i].Ball.X = state[i].Item1.X;
                    this.Balls[i].Ball.Y = state[i].Item1.Y;
                    this.Balls[i].Ball.Vx = state[i].Item2.X;
                    this.Balls[i].Ball.Vy = state[i].Item2.Y;
                }

                this.PostUpdate();
            }
        }

        public void PostUpdate()
        {
            foreach (var b in Balls)
            {
                b.InvalidateVisual();
            }

            this.CalculateEnergy();
        }

        private void MoveBalls(double dt)
        {
            foreach (var b in Balls)
            {
                b.Ball.Update(dt);
            }
        }

        private void OnResetClick(object sender, RoutedEventArgs e)
        {
            this.ResetSystem();
        }

        private void ResetSystem()
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();

                this.Start.IsEnabled = true;
                this.Start.Content = "Start";
            }

            foreach (var b in this.Balls)
            {
                b.Destroy();
            }

            foreach (var e in this.Energy)
            {
                e.Destroy();
            }
            this.Balls.Clear();
            this.Energy.Clear();

            Random r = new Random();
            int n = (int)(this.Number.Value);

            for (int i = 0; (i < n); ++i)
            {
                double radius;
                double x;
                double y;
                bool overlap;
                do
                {
                    radius = (r.NextDouble() + 1.0) * ((i == 0) ? this.SizeRatio.Value : 1.0);

                    x = r.NextDouble() * (100.0 - 2.0 * (radius + 1.0)) + (radius + 1.0);
                    y = r.NextDouble() * (100.0 - 2.0 * (radius + 1.0)) + (radius + 1.0);

                    overlap = false;
                    for (int j = 0; (j < i); ++j)
                    {
                        var dx = this.Balls[j].Ball.X - x;
                        var dy = this.Balls[j].Ball.Y - y;
                        var d2 = dx * dx + dy * dy;
                        var jointRadius = radius + this.Balls[j].Ball.Radius + 2.0;
                        if (d2 < jointRadius * jointRadius)
                        {
                            overlap = true;
                            break;
                        }
                    }
                }
                while (overlap);

                double mass = radius * radius;
                double a = r.NextDouble() * Math.PI * 2.0;

                double e = this.InitialEnergy.Value * ((i == 0) ? this.EnergyRatio.Value : 1.0);
                double pe = -Globals.Gravity * (y - radius) * mass;
                double ke = Math.Max(0.0, e - pe);
                double v = Math.Sqrt(2.0 * ke / mass);

                double vx = v * Math.Cos(a);
                double vy = v * Math.Sin(a);

                var red = (i == 0) ? (byte)(100 + r.Next(50)) : (byte)(r.Next(50));
                Brush b = new SolidColorBrush(Color.FromRgb(red, (byte)(r.Next(200)), (byte)(r.Next(200))));
                this.Balls.Add(new BallSprite(b, this.Surface, radius, radius*radius, x, y, vx, vy));
            }

            double initialEnergy = 0.0;
            foreach (var b in this.Balls)
            {
                initialEnergy += b.Ball.PotentialEnergy + b.Ball.KineticEnergy;
            }

            foreach (var b in this.Balls)
            {
                this.Energy.Add(new EnergySprite(b.Brush, this.Content, b.Ball, initialEnergy));
            }

            this.CalculateEnergy();

            _states.Clear();
            this.AdvanceStates();
        }

        private void CalculateEnergy()
        {
            double currentEnergy = 0.0;
            foreach (var e in this.Energy)
            {
                e.EnergyOffset = currentEnergy;
                currentEnergy += e.Ball.PotentialEnergy + e.Ball.KineticEnergy;

                e.InvalidateVisual();
            }
        }

        private void OnRewindClick(object sender, RoutedEventArgs e)
        {
            if (_timer.IsEnabled)
            {
                this.StopAction();
            }

            this.ReverseTick();
        }

        private void OnTickClick(object sender, RoutedEventArgs e)
        {
            if (_timer.IsEnabled)
            {
                this.StopAction();
            }

            this.AdvanceClock(0.1, breakOnFirstCollision: false);
        }

        private void OnStartClick(object sender, RoutedEventArgs e)
        {
            if (_timer.IsEnabled)
            {
                this.StopAction();
            }
            else
            {
                this.StartAction();
            }
        }

        private void StartAction()
        {
            if (!_timer.IsEnabled)
            {
                if (this.Balls.Count == 0)
                {
                    this.ResetSystem();
                }

                _lastTick = DateTime.Now;

                _timer.Start();

                this.Start.Content = "Stop";
            }

        }

        private void StopAction()
        {
            if (_timer.IsEnabled)
            {
                _timer.Stop();

                this.Start.IsEnabled = true;
                this.Start.Content = "Start";
            }
        }

        private void OnGravityValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Globals.Gravity = -e.NewValue;
        }

        private void OnBallValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Globals.CoefficientOFRestitutionBalls = e.NewValue;
        }
        private void OnFloorValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Globals.CoefficientOFRestitutionTopAndBottom = e.NewValue;
        }
        private void OnWallValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Globals.CoefficientOFRestitutionLeftAndRight = e.NewValue;
        }

        private void OnNumberValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.Count.Text = ((int)(e.NewValue)).ToString() + " balls";
        }

        private void OnSizeRatioValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.SizeR.Text = string.Format("Size ratio {0:F2}:1", e.NewValue);
        }

        private void OnEnergyRatioValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.EnergyR.Text = string.Format("Energy ratio {0:F2}:1", e.NewValue);
        }

        private void OnInitialEnergyValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            this.EnergyI.Text = string.Format("Initial Energy {0:F2} J", e.NewValue);
        }

        public List<Scenario> StartingScenarios = new List<Scenario>()
        {
            new Scenario("BigBig", Tuple.Create(10.0, 25.0, 25.0,  5.0, 10.0),
                                   Tuple.Create(10.0, 75.0, 25.0, -5.0, 10.0)),
            new Scenario("BigsSmall", Tuple.Create(10.0, 25.0, 25.0,  5.0, 10.0),
                                      Tuple.Create(1.0, 75.0, 34.0, -5.0, 10.0))
        };


        public class Scenario
        {
            public string Name { get; }
                                //     r       x       y       vx      vy
            public IReadOnlyList<Tuple<double, double, double, double, double>> States { get; }

            public Scenario(string name, params Tuple<double, double, double, double, double>[] states)
            {
                this.Name = name;
                this.States = states;
            }

            public override string ToString()
            {
                return this.Name;
            }
        }
    }
}
