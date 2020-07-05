using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NodBot.Code;
using System.Threading;
using Microsoft.Win32;
using System.IO;
using NodBot.Code.Services;
using System.Windows.Media;
using NodBot.Code.Overlay;
using Process.NET;
using Process.NET.Memory;

namespace NodBot
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NodBotAI : OverlayWindow
    {
        bool IsNodBotActive = false;
        private SeqBase mCurrentSequence;
        private int grindOption = 0;


        // Window services
        private IOService IOService = new IOService();

        // Sequence Services
        private Logger mLogger;
        private NodiatisInputService mInput;
        private InventoryService mInventory;

        // Overlay data points
        private int kill_count = 0, chest_count = 0;
        private Progress<int> progressKillCount;
        private Progress<int> progressChestCount;
        private Progress<string> progressLog;

        private string[] NodBotOptions = { "Farm", "Town Walk(T4)", "Town Walk(T5)" };

        // Async cancellation token
        private CancellationTokenSource cts;

        /// <summary>
        /// 
        /// </summary>
        public NodBotAI()
        {
            InitializeComponent();
            Init();
            StartOverlay();
            this.Show();
        }

        /// <summary>
        /// 
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            mInput.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        private void Init()
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
            if (options_combo.Items.Count > 0) return;
            options_combo.Items.Add(NodBotOptions[0]);
            options_combo.Items.Add(NodBotOptions[1]);
            options_combo.Items.Add(NodBotOptions[2]);
            options_combo.SelectedIndex = 0;
        }

        #region Overlay Update Events

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

        #endregion Overlay Update Events


        #region Window Even Handlers

        /// <summary>
        /// This function handles click events for the UI Test button.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            hasOverlay = false;

            Task.Run(() =>
            {

                Task.Delay(1000).Wait();
                //new SeqGardening(new CancellationTokenSource(), mLogger).Start().Wait();
                //mInventory.SortInventory().Wait();
                mInventory.GetFirstEmptyStorageSpace();

            });
        }



        /// <summary>
        /// This function handles click events for the UI Start button.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsNodBotActive) stopGrind();
            else startGrind();
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
                    mLogger.info("PILGRIMAGE ON");
                    break;
                case "chest_checkbox":
                    Settings.CHESTS = false; // Will not look for chests
                    mLogger.info("CHESTS OFF");
                    break;
                case "debug_checkbox":
                    Settings.DEBUG = true;
                    mLogger.info("DEBUG ON");
                    border_debug.Visibility = Visibility.Visible;
                    break;
                case "arena_checkbox":
                    Settings.ARENA = true;
                    mLogger.info("ARENA ON");
                    break;
                case "bossing_checkbox":
                    Settings.BOSSING = true;
                    mLogger.info("BOSSING ON");
                    break;
                case "resource_wait":
                    Settings.WAIT_FOR_HEALTH = true;
                    mLogger.info("WAITING FOR RESOURCES ON");
                    break;
                case "resource_wait_mana":
                    Settings.WAIT_FOR_MANA = true;
                    mLogger.info("WAITING FOR RESOURCE MANA ON");
                    break;
                case "resource_wait_energy":
                    Settings.WAIT_FOR_ENERGY = true;
                    mLogger.info("WAITING FOR RESOURCE ENERGY ON");
                    break;
                case "option_mining":
                    Settings.RESOURCE_MINING = true;
                    mLogger.info("MINING ON");
                    break;
                case "option_garden":
                    Settings.RESOURCE_GARDEN = true;
                    mLogger.info("GARDEN ON");
                    break;
                case "manage_inventory":
                    Settings.MANAGE_INVENTORY = true;
                    mLogger.info("MANAGE INVENTORY ON");
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
                    mLogger.info("PILGRIMAGE OFF");
                    break;
                case "chest_checkbox":
                    Settings.CHESTS = true; // Will look for chests  
                    mLogger.info("CHESTS OFF");
                    break;
                case "debug_checkbox":
                    Settings.DEBUG = false;
                    mLogger.info("DEBUG OFF");
                    border_debug.Visibility = Visibility.Hidden;
                    break;
                case "arena_checkbox":
                    Settings.ARENA = false;
                    mLogger.info("ARENA OFF");
                    break;
                case "bossing_checkbox":
                    Settings.BOSSING = false;
                    mLogger.info("BOSSING OFF");
                    break;
                case "resource_wait":
                    Settings.WAIT_FOR_HEALTH = false;
                    mLogger.info("WAITING FOR RESOURCE HEALTH OFF");
                    break;
                case "resource_wait_mana":
                    Settings.WAIT_FOR_MANA = false;
                    mLogger.info("WAITING FOR RESOURCE MANA OFF");
                    break;
                case "resource_wait_energy":
                    Settings.WAIT_FOR_ENERGY = false;
                    mLogger.info("WAITING FOR RESOURCE ENERGY OFF");
                    break;
                case "option_mining":
                    Settings.RESOURCE_MINING = false;
                    mLogger.info("MINING OFF");
                    break;
                case "manage_inventory":
                    Settings.MANAGE_INVENTORY = false;
                    mLogger.info("MANAGE INVENTORY OFF");
                    break;
                case "option_garden":
                    Settings.RESOURCE_GARDEN = false;
                    mLogger.info("GARDEN OFF");
                    break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NeutralMenuItem_Click(object sender, RoutedEventArgs e)
        {
            ImageService test = new ImageService(cts, mLogger, InputService.getNodiatisWindowHandle());
            test.CaputreNeutralPoint();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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
                    IOService.readSettingFile();
                    this.Title = "Player - " + Settings.WINDOW_NAME;
                    Directory.CreateDirectory("Images\\" + Settings.Player.playerName);
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(ex.StackTrace);
            }

            Init();
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

        #endregion Window Even Handlers


        #region Overlay Setup


        /***
         * 
         * 
         * Overlay setup
         * 
         * 
         */
        public event EventHandler<DrawingContext> Draw;
        private bool hasOverlay = false;

        public void StartOverlay()
        {
            var process = System.Diagnostics.Process.GetProcessesByName("Nodiatis").Where(p => p.MainWindowTitle == Settings.Player.playerName).FirstOrDefault();

            if (process == null) return;

            var _processSharp = new ProcessSharp(process, MemoryType.Remote);

            // This is done to focus on the fact the Init method
            // is overriden in the wpf overlay demo in order to set the
            // wpf overlay window instance
            //Initialize(_processSharp.WindowFactory.MainWindow);
            Initialize(_processSharp.WindowFactory.MainWindow);
         
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
        ///     Updates this instance.
        /// </summary>
        public void UpdateWindow()
        {
            try
            {
                if (hasOverlay)
                {
                    Width = TargetWindow.Width;
                    Height = TargetWindow.Height;
                    Left = TargetWindow.X;
                    Top = TargetWindow.Y + 2;
                }
                else UpdateWindow2();
            }
            catch(Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(ex.StackTrace);
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public void UpdateWindow2()
        {
            Width = 180;
            Height = 25;
            Left = TargetWindow.X + (TargetWindow.Width / 2) - 90;
            Top = TargetWindow.Y+2;
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

        /// <summary>
        ///     Called when [draw].
        /// </summary>
        /// <param name="e">The e.</param>
        protected virtual void OnDraw(DrawingContext e) => Draw?.Invoke(this, e);

        private void overlay_Click(object sender, RoutedEventArgs e)
        {
            hasOverlay = !hasOverlay;
        }

        #endregion Overlay Setup


        /// <summary>
        /// This function handles starting the selected bot sequence to execute.
        /// 
        /// </summary>
        private void startGrind()
        {
            try
            {
                mLogger.info("Starting bot");
                start_button.Content = "Stop";
                grindOption = options_combo.SelectedIndex;


                cts = new CancellationTokenSource();

                cts.Token.Register(() => { stopGrind(); });

                Task.Run(async () =>
                {
                    IsNodBotActive = true;
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
            catch (Exception ex)
            {
                mLogger.error(ex);
            }
        }

        /// <summary>
        /// This function handles stopping the currently running bot sequence.
        /// 
        /// </summary>
        private void stopGrind()
        {
            mLogger.info("Stopping bot");
            this.Dispatcher.Invoke(() =>
            {
                start_button.Content = "Start";
                IsNodBotActive = false;
                if (cts != null)
                {
                    cts.Cancel();
                    cts = null;
                }
            });
        }
    }
}
