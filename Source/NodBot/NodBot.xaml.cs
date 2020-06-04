using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NodBot.Code;
using System.Threading;
using Microsoft.Win32;
using System.IO;
using NodBot.Model;
using Newtonsoft.Json;
using System.Drawing;
using NodBot.Code.Services;
using Emgu.CV;
using System.Collections;
using NodBot.Code.Enums;
using System.Windows.Media;
using System.Runtime.InteropServices;
using Size = System.Windows.Size;
using System.Windows.Interop;
using NodBot.Code.Overlay;
using Process.NET.Windows;
using Process.NET;
using Process.NET.Memory;

namespace NodBot
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NodBotAI : OverlayPlugin
    {
        bool isRunning = false;

        private Logger mLogger;
        private NodiatisInputService mInput;
        private InventoryService mInventory;

        private int kill_count = 0, chest_count = 0;
        private CancellationTokenSource cts;
        private bool isSequenceInit = false;

        private Progress<int> progressKillCount;
        private Progress<int> progressChestCount;
        private Progress<string> progressLog;
        private string[] mNodBotOptions = { "Farm", "Town Walk(T4)", "Town Walk(T5)" };

        public Boolean IsActivated { get; set; }



        private void init()
        {
            
            this.Title = "Player - " + Settings.Player.playerName;

            // init logger
            progressKillCount = new Progress<int>(value => updateKillCount());
            progressChestCount = new Progress<int>(value => updateChestCount());
            progressLog = new Progress<string>(value => updateLog(value));

            mLogger = new Logger(progressLog);
            mInput = new NodiatisInputService(mLogger);
            mInventory = new InventoryService(mInput.inputService);

            // Component inits
            log_textbox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            options_combo.Items.Add(mNodBotOptions[0]);
            options_combo.Items.Add(mNodBotOptions[1]);
            options_combo.Items.Add(mNodBotOptions[2]);
            options_combo.SelectedIndex = 0;
        }

        private void initSequences()
        {
            if (isSequenceInit) return;
            isSequenceInit = true;

        }

        CancellationTokenSource inventoryCancel = new CancellationTokenSource();

        /// <summary>
        /// This function handles click events for the UI Test button.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {



        }

        /// <summary>
        /// This function handles click events for the UI Start button.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning) stopGrind();
            else startGrind();
        }

        private SeqBase mCurrentSequence;
        private int grindOption = 0;

        /// <summary>
        /// This function handles starting the selected bot sequence to execute.
        /// 
        /// </summary>
        private void startGrind()
        {
            initSequences();

            try
            {
                mLogger.sendMessage("Starting bot", LogType.INFO);
                start_button.Content = "Stop";
                grindOption = options_combo.SelectedIndex;
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.StackTrace);
            }

            cts = new CancellationTokenSource();

            cts.Token.Register(() =>
            {
                if (!isManualStop) startGrind();
                else Console.Out.WriteLine("Stop button was pressed");

                isManualStop = false;
            });

            Task.Run(async () =>
            {
                isRunning = true;
                if (grindOption == 0)
                {
                    mCurrentSequence = new SeqGrind(cts, mLogger, progressKillCount, progressChestCount);
                    await mCurrentSequence.Start();
                }
                else if (grindOption == 2 || grindOption == 3)
                {
                    mCurrentSequence = new SeqTownWalk(cts, mLogger);
                    await ((SeqTownWalk)mCurrentSequence).Start(grindOption == 3);
                }
            });
        }

        bool isManualStop = false;
        /// <summary>
        /// This function handles stopping the currently running bot sequence.
        /// 
        /// </summary>
        private void stopGrind()
        {
            isManualStop = true;
            mLogger.sendMessage("Stopping bot", LogType.INFO);
            this.Dispatcher.Invoke(() =>
            {
                start_button.Content = "Start";
                isRunning = false;
                if (cts != null)
                {
                    cts.Cancel();
                    cts = null;
                }
            });
        }

        /// <summary>
        /// This event handler toggles the Settings based on the events 
        /// of from the various CheckBoxes in the UI.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            switch (((CheckBox)sender).Name)
            {
                case "pilgrimage_checkbox":
                    Settings.PILGRIMAGE = true; // Is on a pilgrimage
                    mLogger.sendMessage("PILGRIMAGE ON", LogType.INFO);
                    break;
                case "chest_checkbox":
                    Settings.CHESTS = false; // Will not look for chests
                    mLogger.sendMessage("CHESTS OFF", LogType.INFO);
                    break;
                case "debug_checkbox":
                    Settings.DEBUG = true;
                    mLogger.sendMessage("DEBUG ON", LogType.INFO);
                    break;
                case "arena_checkbox":
                    Settings.ARENA = true;
                    break;
                case "bossing_checkbox":
                    Settings.BOSSING = true;
                    break;
                case "resource_wait":
                    Settings.WAIT_FOR_RESOURCES = true;
                    break;
                case "option_mining":
                    Settings.RESOURCE_MINING = true;
                    break;
                case "manage_inventory":
                    Settings.MANAGE_INVENTORY = true;
                    break;
            }
        }

        /// <summary>
        /// This event handler toggles the Settings based on the events of from 
        /// the various CheckBoxes in the UI.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_UnChecked(object sender, RoutedEventArgs e)
        {
            switch (((CheckBox)sender).Name)
            {
                case "pilgrimage_checkbox":
                    Settings.PILGRIMAGE = false; // Is not on a pilgrimage
                    mLogger.sendMessage("PILGRIMAGE OFF", LogType.INFO);
                    break;
                case "chest_checkbox":
                    Settings.CHESTS = true; // Will look for chests  
                    mLogger.sendMessage("CHESTS OFF", LogType.INFO);
                    break;
                case "debug_checkbox":
                    Settings.DEBUG = false;
                    mLogger.sendMessage("DEBUG OFF", LogType.INFO);
                    break;
                case "arena_checkbox":
                    Settings.ARENA = false;
                    break;
                case "bossing_checkbox":
                    Settings.BOSSING = false;
                    break;
                case "resource_wait":
                    Settings.WAIT_FOR_RESOURCES = false;
                    break;
                case "option_mining":
                    Settings.RESOURCE_MINING = false;
                    break;
                case "manage_inventory":
                    Settings.MANAGE_INVENTORY = false;
                    break;

            }
        }

        /// <summary>
        /// This function updates the UI kill counter.
        /// 
        /// </summary>
        private void updateKillCount()
        {
            kill_count++;
            kill_counter_label.Content = kill_count;
        }

        /// <summary>
        /// This function updates the UI chest counter.
        /// 
        /// </summary>
        private void updateChestCount()
        {
            chest_count++;
            chest_counter_label.Content = chest_count;
        }

        /// <summary>
        /// This function clears the textbox log of all its content.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            log_textbox.Clear();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            new NodBotInit().Show();

            stopGrind();
            this.Close();
        }

        /// <summary>
        /// This function adds messages to the textbox log on the GUI.
        /// 
        /// </summary>
        /// <param name="aMessage"></param>
        public void updateLog(string aMessage)
        {
            if (log_textbox.LineCount >= log_textbox.MaxLines)
            {
                string[] lines = log_textbox.Text.Split(Environment.NewLine.ToCharArray()).Skip(1).ToArray();
                log_textbox.Text = string.Join("\n", lines);
            }

            log_textbox.Text += aMessage + "\n";
            log_textbox.ScrollToEnd();
        }

        private void ProfileMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var fileContent = string.Empty;
            var filePath = string.Empty;

            OpenFileDialog openFileDialog = new OpenFileDialog();

            try
            {
                openFileDialog.InitialDirectory = Directory.GetCurrentDirectory() + "\\Settings";
                openFileDialog.Filter = "nodbot files (*.nb)|*.nb|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;

                if (openFileDialog.ShowDialog() == true)
                {
                    //Get the path of specified file
                    filePath = openFileDialog.FileName;
                    Settings.SETTINGS_FILE = openFileDialog.FileName;
                    readSettingFile();
                    this.Title = "Player - " + Settings.WINDOW_NAME;
                    System.IO.Directory.CreateDirectory("Images\\" + Settings.Player.playerName);
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(ex.StackTrace);
            }

            init();
        }

        private void NeutralMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImageService test = new ImageService(cts, mLogger, InputService.getNodiatisWindowHandle());
            test.CaputreNeutralPoint();
        }

        private void readSettingFile()
        {
            string[] lines = System.IO.File.ReadAllLines(Settings.SETTINGS_FILE);

            string json = File.ReadAllText(Settings.SETTINGS_FILE);
            PlayerSettings settings = DeserializePlayerSettings(json);

            string test = SerializePlayerSettings(settings);

            if (settings.playerName == null || settings.playerName.Length == 0)
            {
                // Player Name required
                return;
            }

            Settings.Player = settings;
            Settings.WINDOW_NAME = settings.playerName;
        }

        public PlayerSettings DeserializePlayerSettings(string json)
        {
            return JsonConvert.DeserializeObject<PlayerSettings>(json);
        }

        private void options_combo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        public String SerializePlayerSettings(PlayerSettings settings)
        {
            return JsonConvert.SerializeObject(settings);
        }



        /***
         * 
         * 
         * Overlay setup
         * 
         * 
         */
        private IWindow _targetWindow;

        public NodBotAI()
        {
            InitializeComponent();
            init();
            DumbStartOverlay();
            this.Show();
        }

        public void StartOverlay()
        {
            var process = System.Diagnostics.Process.GetProcessesByName("Nodiatis").Where(p => p.MainWindowTitle == Settings.Player.playerName).FirstOrDefault();

            if (process == null) return;

            var _processSharp = new ProcessSharp(process, MemoryType.Remote);
            // var _overlay = new OverlayImpl();

            // var wpfOverlay = (OverlayImpl)_overlay;

            // This is done to focus on the fact the Init method
            // is overriden in the wpf overlay demo in order to set the
            // wpf overlay window instance
            Initialize(_processSharp.WindowFactory.MainWindow);
            Enable();

            while (true)
            {
                Update();
            }
        }

        public void DumbStartOverlay()
        {
            var process = System.Diagnostics.Process.GetProcessesByName("Nodiatis").Where(p => p.MainWindowTitle == Settings.Player.playerName).FirstOrDefault();

            if (process == null) return;

            var _processSharp = new ProcessSharp(process, MemoryType.Remote);
            _targetWindow = _processSharp.WindowFactory.MainWindow;
            TargetWindow = _targetWindow;

            _tickEngine.Interval = TimeSpan.FromMilliseconds(updateRate);
            _tickEngine.PreTick += OnPreTick;
            _tickEngine.Tick += OnTick;


            //StartOverlay();
            UpdateWindow();
            Enable();

            var task = Task.Run(() =>
            {
                while (true)
                {
                    Update();
                }
            });
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OverlayWindow" /> class.
        /// </summary>
        /// <param name="targetWindow">The window.</param>
        public NodBotAI(IWindow targetWindow)
        {
            InitializeComponent();
            init();
            StartOverlay();
        }

        public event EventHandler<DrawingContext> Draw;

        /// <summary>
        ///     Updates this instance.
        /// </summary>
        public void UpdateWindow()
        {
            if (hasOverlay)
            {
                Width = _targetWindow.Width;
                Height = _targetWindow.Height;
                Left = _targetWindow.X;
                Top = _targetWindow.Y;
            }
            else UpdateWindow2();
        }

        public void UpdateWindow2()
        {
            Width = 180;
            Height = 25;
            Left = _targetWindow.X + (_targetWindow.Width / 2) - 90;
            Top = _targetWindow.Y;
        }

        /// <summary>
        ///     Raises the <see cref="E:System.Windows.Window.SourceInitialized" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            // We need to do this in order to allow shapes
            // drawn on the canvas to be click-through. 
            // There is no other way to do this.
            // Source: https://social.msdn.microsoft.com/Forums/en-US/c32889d3-effa-4b82-b179-76489ffa9f7d/how-to-clicking-throughpassing-shapesellipserectangle?forum=wpf
           // this.MakeWindowTransparent();
        }

        /// <summary>
        ///     When overridden in a derived class, participates in rendering operations that are directed by the layout system.
        ///     The rendering instructions for this element are not used directly when this method is invoked, and are instead
        ///     preserved for later asynchronous use by layout and drawing.
        /// </summary>
        /// <param name="drawingContext">
        ///     The drawing instructions for a specific element. This context is provided to the layout
        ///     system.
        /// </param>
        protected override void OnRender(DrawingContext drawingContext)
        {
            OnDraw(drawingContext);
            base.OnRender(drawingContext);
        }

        /// <summary>
        ///     Adds the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        public void Add(UIElement element) => OverlayGrid.Children.Add(element);

        /// <summary>
        ///     Removes the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        public void Remove(UIElement element) => OverlayGrid.Children.Remove(element);

        /// <summary>
        ///     Adds the specified element.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="index">The index.</param>
        public void Add(UIElement element, int index) => OverlayGrid.Children[index] = element;

        /// <summary>
        ///     Removes the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        public void Remove(int index) => OverlayGrid.Children.RemoveAt(index);










       












        // Used to limit update rates via timestamps 
        // This way we can avoid thread issues with wanting to delay updates
        private readonly TickEngine _tickEngine = new TickEngine();



        private int updateRate = 1000 / 60;


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
            _targetWindow = targetWindow;

            //OverlayWindow = new NodBotAI(targetWindow);
            Show();


            // Set up update interval and register events for the tick engine.
            _tickEngine.Interval = TimeSpan.FromMilliseconds(updateRate);
            //_tickEngine.PreTick += OnPreTick;
            _tickEngine.Tick += OnTick;
        }

        private void OnTick(object sender, EventArgs eventArgs)
        {
            // This will only be true if the target window is active
            // (or very recently has been, depends on your update rate)
            if (IsVisible)
            {
                UpdateWindow();
            }
        }

        private void OnPreTick(object sender, EventArgs eventArgs)
        {
            //var activated = ((RemoteWindow)TargetWindow).IsActivated;
            var targetActivated = TargetWindow.IsActivated;
            var visible = IsVisible;
            var activated = IsActivated;

            // Ensure window is shown or hidden correctly prior to updating
            if (!targetActivated  && !activated && visible)
            {
                //Console.Out.WriteLine("HIDE :: {0} :: {1}", !targetActivated, visible);
                //Hide();
            }

            else if (targetActivated && !visible)
            {
                Console.Out.WriteLine("SHOW :: {0} :: {1}", targetActivated, !visible);
                Show();
            }
        }

        public override void Update() => _tickEngine.Pulse();

        bool hasOverlay = false;
        private void overlay_Click(object sender, RoutedEventArgs e)
        {

            if (!hasOverlay)
            {
                hasOverlay = true;
            }
            else
            {
                hasOverlay = false;
            }
        }


        /// <summary>
        ///     Called when [draw].
        /// </summary>
        /// <param name="e">The e.</param>
        protected virtual void OnDraw(DrawingContext e) => Draw?.Invoke(this, e);


    }
}
