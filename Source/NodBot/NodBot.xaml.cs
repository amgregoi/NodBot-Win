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
        private int kill_count = 0, chest_count = 0;
        private CancellationTokenSource cts;
        private bool isSequenceInit = false;

        private Progress<int> progressKillCount;
        private Progress<int> progressChestCount;
        private Progress<string> progressLog;
        private InventoryService inventoryService;
        private string[] mNodBotOptions = { "Farm", "Arena", "Town Walk(T4)" , "Town Walk(T5)"};

        public NodBotAI()
        {
            InitializeComponent();
            init();
        }

        private void init()
        {
            this.Title = "Player - " + Settings.Player.playerName;

            // init logger
            progressKillCount = new Progress<int>(value => updateKillCount());
            progressChestCount = new Progress<int>(value => updateChestCount());
            progressLog = new Progress<string>(value => updateLog(value));

            mLogger = new Logger(progressLog);

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

            mGrindSequence = new SeqGrind(mLogger, progressKillCount, progressChestCount);
            mTownWalkSequence = new SeqTownWalk(mLogger);
            mArenaSequence = new SeqArena(mLogger);
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
            /*
            SeqArena test = new SeqArena(mLogger);
            Task.Delay(25).ContinueWith(async _ =>
            {
                await test.EnterQueue();
            });
            */


            ImageAnalyze test = new ImageAnalyze(mLogger);
            ImageAnalyze.CaptureScreen();
            if (inventoryService != null)
            {
                Task.Run(() =>
                {
                    inventoryCancel.Cancel();
                    try
                    {
                        inventoryService.stackItems(NodImages.SDread_Trophy1).Wait(inventoryCancel.Token);
                        inventoryService.stackItems(NodImages.SDread_Trophy2).Wait(inventoryCancel.Token);
                        inventoryService.stackItems(NodImages.SDread_Trophy3).Wait(inventoryCancel.Token);
                        inventoryService.stackItems(NodImages.SDread_Trophy4).Wait(inventoryCancel.Token);
                    }
                    catch (AggregateException ex)
                    {
                        Console.Out.WriteLine(ex.StackTrace);
                        Console.Out.WriteLine(ex.InnerException.StackTrace);
                    }
                });
            }
            else
            {
                Task.Run(() =>
                {
                    inventoryService = new InventoryService(new Input(Settings.WINDOW_NAME, mLogger));
                });
            }
            //var point = new ImageAnalyze(mLogger).FindMatchTemplate(NodImages.Temp_Inventory_1, NodImages.Gate);            
            
            //test.findMatchTest(NodImages.CurrentSS, NodImages.SDread_Trophy1, Color.Red, Color.Red);

            // Console.Out.WriteLine(result);
            // mLogger.sendLog(result.ToString(), LogType.INFO);

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

        /// <summary>
        /// This function handles starting the selected bot sequence to execute.
        /// 
        /// </summary>
        private void startGrind()
        {
            initSequences();

            mLogger.sendMessage("Starting bot", LogType.INFO);
            start_button.Content = "Stop";
            int optionsIndex = options_combo.SelectedIndex;

            cts = new CancellationTokenSource();

            cts.Token.Register(() =>
            {
                stopGrind();
            });

            Task.Run(async () =>
            {
                isRunning = true;
                if(optionsIndex == 0) await mGrindSequence.Start(cts.Token);
                else if (optionsIndex == 1) await mArenaSequence.Start(cts.Token); // start arena
                else if (optionsIndex == 2) await mTownWalkSequence.Start(cts.Token, false); // start north towards T4
                else if (optionsIndex == 3) await mTownWalkSequence.Start(cts.Token, true); // start south towards T5
            });
        }

        /// <summary>
        /// This function handles stopping the currently running bot sequence.
        /// 
        /// </summary>
        private void stopGrind()
        {
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
            }
        }

        /// <summary>
        /// This event handler toggles the Auto Attack and Class Ability options.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            switch (((Slider)sender).Name)
            {
                case "attack_slider":
                    if (e.NewValue > 0)
                    {
                        Settings.MELEE = false;
                        mLogger.sendMessage("ATTACK SET [S]", LogType.INFO);
                    }
                    else
                    {
                        Settings.MELEE = true;
                        mLogger.sendMessage("ATTACK SET [A]", LogType.INFO);
                    }
                    break;
                case "ability_slider":
                    if (e.NewValue > 0)
                    {
                        Settings.CA_PRIMARTY = false;
                        mLogger.sendMessage("CLASS ABILITY SET [F]", LogType.INFO);
                    }
                    else
                    {
                        Settings.CA_PRIMARTY = true;
                        mLogger.sendMessage("ATTACK SET [D]", LogType.INFO);
                    }
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
            if (log_textbox.LineCount >= log_textbox.MaxLines) {
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
            ImageAnalyze test = new ImageAnalyze();
            ImageAnalyze.CaputreNeutralPoint();
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
    }
}
