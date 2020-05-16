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

namespace NodBot
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class NodBotAI : Window
    {
        bool isRunning = false;

        private SeqGrind mGrindSequence;
        private SeqTownWalk mTownWalkSequence;
        private SeqArena mArenaSequence;

        private Logger mLogger;
        private NodiatisInputService mInput;
        private InventoryService mInventory;

        private int kill_count = 0, chest_count = 0;
        private CancellationTokenSource cts;
        private bool isSequenceInit = false;

        private Progress<int> progressKillCount;
        private Progress<int> progressChestCount;
        private Progress<string> progressLog;
        private string[] mNodBotOptions = { "Farm", "Arena", "Town Walk(T4)", "Town Walk(T5)" };

        public NodBotAI()
        {
            InitializeComponent();
            init();
        }

        private void init()
        {
            this.Title = "Player - " + Settings.Player.playerName;

            GetSupportedTrophyZones();

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
            options_combo.Items.Add(mNodBotOptions[3]);
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
            ImageService test = new ImageService(cts, mLogger, InputService.getNodiatisWindowHandle());

          

            if (mInventory != null) mInventory.sortInventory();
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
                else if (grindOption == 1)
                {
                    mCurrentSequence = new SeqArena(cts, mLogger);
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

        public String SerializePlayerSettings(PlayerSettings settings)
        {
            return JsonConvert.SerializeObject(settings);
        }

        private void options_combo_zones_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            Settings.ZONE = (String) options_combo_zones.Items.GetItemAt(options_combo_zones.SelectedIndex);
        }

        private void GetSupportedTrophyZones()
        {
            // Process the list of files found in the directory.
            string[] folderEntries = Directory.GetDirectories("Images\\trophy");
            foreach (String zone in folderEntries)
            {
                var title = zone.Split(new char[] { '\\' }).Last();
                options_combo_zones.Items.Add(title);
            }

            options_combo_zones.SelectedIndex = 0;
        }
    }
}
