using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Glider
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _started = false;
        private bool _paused = false;

        private int _score;
        private int _lives = 10;

        public int Score
        {
            get { return _score; }
            set
            {
                _score = value;
                this.ScoreCounter.Text = _score.ToString();
            }
        }

        public int Lives
        {
            get { return _lives; }
            set
            {
                _lives = value;
                this.LivesCounter.Text = _lives.ToString();
            }
        }

        private Player Player;
        private List<Rock> Rocks = new List<Rock>();

        public static Random Random = new System.Random();

        DispatcherTimer _timer;
        int _tickCount = 0;

        DateTime _lastTick;

        public MainWindow()
        {
            InitializeComponent();

            _timer = new DispatcherTimer(DispatcherPriority.Input);
            _timer.Interval = TimeSpan.FromMilliseconds(50.0);
            _timer.Tick += this.OnTick;
        }

        private void OnStartClick(object sender, RoutedEventArgs e)
        {
            if (_started)
            {
                this.Stop();
            }

            this.Start();
        }

        private void Start()
        {
            _started = true;

            this.Score = 0;
            this.Lives = 10;

            this.StartButton.Content = "Restart";

            if (this.Player != null)
                this.Player.Dispose();
            this.Player = new Player(this.Map);

            foreach (var r in this.Rocks)
                r.Dispose();
            this.Rocks.Clear();

            for (int i = 0; (i < 25); ++i)
            {
                this.Rocks.Add(new Rock(this.Map));
            }

            this.StartClock();
        }

        private void Stop()
        {
            _started = false;

            this.StopClock();
            this.StartButton.Content = "Restart";
        }

        private void StartClock()
        {
            _paused = false;

            _lastTick = DateTime.UtcNow;
            _timer.Start();
        }

        private void StopClock()
        {
            _paused = true;

            _timer.Stop();
        }

        private void OnTick(object sender, EventArgs args)
        {
            if (!_paused)
            {
                DateTime tick = DateTime.UtcNow;

                TimeSpan elapsed = tick - _lastTick;
                _lastTick = tick;

                this.Player.Tick(elapsed);
                if ((++_tickCount) % 2 == 0)
                {
                    this.Rocks.Add(new Rock(this.Map));
                }

                for (int i = 0; (i < this.Rocks.Count); ++i)
                {
                    Rock r = this.Rocks[i];

                    if (r.Tick(elapsed, this.Player))
                    {
                        --(this.Lives);

                        if (this.Lives <= 0)
                        {
                            this.Stop();
                        }
                    }
                    
                    if (r.Position.X > this.Map.ActualWidth)
                    {
                        r.Dispose();
                        this.Rocks[i] = new Rock(this.Map);

                        if (r.IsAlive)
                            ++(this.Score);
                    }
                }
            }
        }
    }

    class Blob : IDisposable
    {
        public readonly Canvas Container;
        public readonly Shape Graphic;

        private Point _position;
        public Point Position
        {
            get { return _position; }
            set
            {
                _position = value;
                Canvas.SetLeft(this.Graphic, _position.X - this.Graphic.Width * 0.5);
                Canvas.SetTop(this.Graphic, _position.Y - this.Graphic.Height * 0.5);
            }
        }

        public Blob(Canvas container, Shape graphic, Point position)
        {
            this.Container = container;
            this.Graphic = graphic;
            this.Position = position;

            container.Children.Add(graphic);
        }

        public void Dispose()
        {
            this.Container.Children.Remove(this.Graphic);
        }
    }

    class Player : IDisposable
    {
        private readonly Blob _blob;
        private readonly Rectangle _ship;

        const double Velocity = 0.25;

        public Player(Canvas canvas)
        {
            _ship = new Rectangle();
            _ship.Width = 50.0;
            _ship.Height = 25.0;
            _ship.Fill = Brushes.Red;

            _ship.VerticalAlignment = VerticalAlignment.Center;
            _ship.HorizontalAlignment = HorizontalAlignment.Center;

            _ship.IsHitTestVisible = false;

            _blob = new Blob(canvas, _ship, new Point(canvas.ActualWidth * 0.75, canvas.ActualHeight * 0.5));
        }

        public Point Position { get { return _blob.Position; } }

        public void Tick(TimeSpan elapsed)
        {
            Point mouse = Mouse.GetPosition(_blob.Container);

            mouse.X = Math.Max(Math.Min(mouse.X, _blob.Container.ActualWidth), _blob.Container.ActualWidth * 0.5);
            mouse.Y = Math.Max(Math.Min(mouse.Y, _blob.Container.ActualHeight), 0.0);

            double dx = mouse.X - _blob.Position.X;
            double dy = mouse.Y - _blob.Position.Y;

            double l = Math.Sqrt(dx * dx + dy * dy);

            double d = elapsed.Milliseconds * Velocity;

            if (l < d)
            {
                _blob.Position = mouse;
            }
            else
            {
                _blob.Position = new Point(_blob.Position.X + dx * d / l, _blob.Position.Y + dy * d / l);
            }
        }

        public void Dispose()
        {
            _blob.Dispose();
        }
    }

    class Rock : IDisposable
    {
        private readonly Blob _blob;
        private readonly Ellipse _rock;

        private bool _isAlive = true;

        Point Velocity;

        public bool IsAlive { get { return _isAlive; } }

        public Rock(Canvas canvas)
        {
            _rock = new Ellipse();
            _rock.Width = 25.0;
            _rock.Height = 25.0;
            _rock.Fill = Brushes.White;

            _rock.VerticalAlignment = VerticalAlignment.Center;
            _rock.HorizontalAlignment = HorizontalAlignment.Center;

            _rock.IsHitTestVisible = false;

            Point position = new Point(0.0, MainWindow.Random.NextDouble() * canvas.ActualHeight);

            Point target;
            double o = MainWindow.Random.NextDouble() * (canvas.ActualHeight + canvas.ActualWidth);
            if (o < canvas.ActualWidth)
            {
                if (o < canvas.ActualWidth * 0.5)
                {
                    target = new Point(o + canvas.ActualWidth * 0.5, 0.0);
                }
                else
                {
                    target = new Point(o, canvas.ActualHeight);
                }
            }
            else
            {
                target = new Point(canvas.ActualWidth, o - canvas.ActualWidth);
            }

            double dx = target.X - position.X;
            double dy = target.Y - position.Y;
            double l = Math.Sqrt(dx * dx + dy * dy);
            double v = 0.5 * (MainWindow.Random.NextDouble() + 0.5) / l;

            this.Velocity = new Point(dx * v, dy * v);

            _blob = new Blob(canvas, _rock, position);
        }

        public Point Position { get { return _blob.Position; } }

        public bool Tick(TimeSpan elapsed, Player player)
        {
            double d = elapsed.Milliseconds;

            _blob.Position = new Point(_blob.Position.X + this.Velocity.X * d, _blob.Position.Y + this.Velocity.Y * d);

            double dx = player.Position.X - _blob.Position.X;
            double dy = player.Position.Y - _blob.Position.Y;
            double rSquared = dx * dx + dy * dy;

            if (_isAlive)
            {
                if (rSquared < 25.0 * 25.0)
                {
                    _rock.Fill = Brushes.Orange;
                    _isAlive = false;

                    return true;
                }
            }
            else
            {
                _rock.Opacity = _rock.Opacity * 0.9;

                Point pt = _blob.Position;
                _rock.Width = _rock.Height = _rock.Width * 1.05;
                _blob.Position = pt;
            }

            return false;
        }

        public void Dispose()
        {
            _blob.Dispose();
        }
    }
}
