using Process.NET.Windows;
using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

using OverlayWindow = NodBot.NodBotAI;

namespace NodBot.Code.Overlay
{

    public class OverlayWindow : WpfOverlayPlugin
    {
        // Used to limit update rates via timestamps 
        // This way we can avoid thread issues with wanting to delay updates
        private readonly TickEngine _tickEngine = new TickEngine();
        private Ellipse _ellipse;

        private bool _isDisposed;

        private bool _isSetup;

        private int updateRate = 1000 / 60;

        // Shapes used in the demo
        private Line _line;
        private Polygon _polygon;
        private Rectangle _rectangle;

        public override void Enable()
        {
            _tickEngine.IsTicking = true;
            base.Enable();
        }

        public override void Disable()
        {
            _tickEngine.IsTicking = false;
            base.Disable();
        }

        public override void Initialize(IWindow targetWindow)
        {
            // Set target window by calling the base method
            base.Initialize(targetWindow);

            // Setup which overlay you want to display
            // OverlayWindow -> Example overlay
            // NodBotAi -> User interface overlay
            OverlayWindow = new NodBotAI(targetWindow);
            OverlayWindow.Show();

            // For demo, show how to use settings
            var type = GetType();

            // Set up update interval and register events for the tick engine.
            _tickEngine.Interval = TimeSpan.FromMilliseconds(updateRate);
            _tickEngine.PreTick += OnPreTick;
            _tickEngine.Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs eventArgs)
        {
            // This will only be true if the target window is active
            // (or very recently has been, depends on your update rate)
            if (OverlayWindow.IsVisible)
            {
                OverlayWindow.UpdateWindow();
            }
        }

        private void OnPreTick(object sender, EventArgs eventArgs)
        {
            // Only want to set them up once.
            if (!_isSetup)
            {
                //SetUp();
                _isSetup = true;
            }
;
            //var activated = ((RemoteWindow)TargetWindow).IsActivated;
            var targetActivated = TargetWindow.IsActivated;
            var visible = OverlayWindow.IsVisible;
            var activated = OverlayWindow.IsActivated;

            // Ensure window is shown or hidden correctly prior to updating
            if (!targetActivated  && !activated && visible)
            {
                Console.Out.WriteLine("HIDE :: {0} :: {1}", !targetActivated, visible);
                OverlayWindow.Hide();
            }

            else if (targetActivated && !visible)
            {
                Console.Out.WriteLine("SHOW :: {0} :: {1}", targetActivated, !visible);
                OverlayWindow.Show();
            }
        }

        public override void Update() => _tickEngine.Pulse();

        // Clear objects
        public override void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }

            if (IsEnabled)
            {
                Disable();
            }

            OverlayWindow?.Hide();
            OverlayWindow?.Close();
            OverlayWindow = null;
            _tickEngine.Stop();

            base.Dispose();
            _isDisposed = true;
        }

        ~OverlayWindow()
        {
            Dispose();
        }

        // Random shapes.. thanks Julian ^_^
        private void SetUp()
        {
            _polygon = new Polygon
            {
                Points = new PointCollection(5) {
                    new Point(100, 150),
                    new Point(120, 130),
                    new Point(140, 150),
                    new Point(140, 200),
                    new Point(100, 200)
                },
                Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 255)),
                Fill =
                    new RadialGradientBrush(
                        Color.FromRgb(255, 255, 0),
                        Color.FromRgb(255, 0, 255))
            };

            OverlayWindow.Add(_polygon);
            _polygon.MouseUp += new MouseButtonEventHandler(MoseUpEvent);

            // Create a line
            _line = new Line
            {
                X1 = 100,
                X2 = 300,
                Y1 = 200,
                Y2 = 200,
                Stroke = new SolidColorBrush(Color.FromRgb(0, 255, 0)),
                StrokeThickness = 2
            };

            _line.MouseUp += new MouseButtonEventHandler(MoseUpEvent);

            OverlayWindow.Add(_line);

            // Create an ellipse (circle)
            _ellipse = new Ellipse
            {
                Width = 15,
                Height = 15,
                Margin = new Thickness(300, 300, 0, 0),
                Stroke =
                    new SolidColorBrush(Color.FromRgb(0, 255, 255))
            };

            OverlayWindow.Add(_ellipse);
            _ellipse.MouseUp += new MouseButtonEventHandler(MoseUpEvent);

            // Create a rectangle
            _rectangle = new Rectangle
            {
                RadiusX = 2,
                RadiusY = 2,
                Width = 50,
                Height = 100,
                Margin = new Thickness(400, 400, 0, 0),
                Stroke = new SolidColorBrush(Color.FromRgb(255, 0, 0)),
                Fill =
                    new SolidColorBrush(Color.FromArgb(100, 255, 255,
                        255))
            };

            OverlayWindow.Add(_rectangle);
            _rectangle.MouseUp += new MouseButtonEventHandler(MoseUpEvent);

        }

        private void MoseUpEvent(object sender, MouseButtonEventArgs e)
        {
            Console.Out.WriteLine("Wooo :: {0}", sender);
        }
    }
}
