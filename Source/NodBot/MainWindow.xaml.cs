using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using NodBot.Code;
using System.Threading;

namespace NodBot
{

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool isRunning = false;

        private SeqGrind mGrindSequence;
        private SeqTownWalk mTownWalkSequence;
        private Logger mLogger;
        private int kill_count = 0, chest_count = 0;
        private CancellationTokenSource cts;

        private string[] mNodBotOptions = { "Farm", "Town Walk(T4)" , "Town Walk(T5)"};


        public MainWindow()
        {
            InitializeComponent();

            var progressKillCount = new Progress<int>(value =>
            {
                updateKillCount();
            });

            var progressChestCount = new Progress<int>(value =>
            {
                updateChestCount();
            });

            var progressLog = new Progress<string>(value =>
            {
                updateLog(value);
            });

            mLogger = new Logger(progressLog);
            mGrindSequence = new SeqGrind(mLogger, progressKillCount, progressChestCount);
            mTownWalkSequence = new SeqTownWalk(mLogger);

            // Component inits
            log_textbox.VerticalScrollBarVisibility = ScrollBarVisibility.Visible;
            options_combo.Items.Add(mNodBotOptions[0]);
            options_combo.Items.Add(mNodBotOptions[1]);
            options_combo.Items.Add(mNodBotOptions[2]);
            options_combo.SelectedIndex = 0;
        }

        /// <summary>
        /// This function handles click events for the UI Test button.
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            ImageAnalyze test = new ImageAnalyze(mLogger);
            ImageAnalyze.CaptureScreen();
            System.Drawing.Point? result = test.FindImageMatchDebug(NodImages.Town5, NodImages.CurrentSS, false);

            Console.Out.WriteLine(result);
            mLogger.sendLog(result.ToString(), LogType.INFO);
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
            mLogger.sendMessage("Starting bot", LogType.INFO);
            start_button.Content = "Stop";
            int optionsIndex = options_combo.SelectedIndex;
            Task.Run(async () =>
            {
                cts = new CancellationTokenSource();
                isRunning = true;
                if(optionsIndex == 0) await mGrindSequence.Start(cts.Token);
                else if (optionsIndex == 1) await mTownWalkSequence.Start(cts.Token, false); // start north towards T4
                else if (optionsIndex == 2) await mTownWalkSequence.Start(cts.Token, true); // start south towards T5
            });
        }

        /// <summary>
        /// This function handles stopping the currently running bot sequence.
        /// 
        /// </summary>
        private void stopGrind()
        {
            mLogger.sendMessage("Stopping bot", LogType.INFO);
            start_button.Content = "Start";
            Task.Delay(50).ContinueWith(_ =>
            {
                isRunning = false;
                cts.Cancel();
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
            chest_counter_label.Content = kill_count;
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
    }
}
