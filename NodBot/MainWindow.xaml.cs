using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Drawing;
using NodBot.Code;
using Emgu.CV;
using System.IO;

namespace NodBot
{

    public interface UIKillCounter
    {
        void updateKillCount();
    }

    public interface UILogMessenger
    {
        void updateLog(String aMessage);
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, UIKillCounter, UILogMessenger
    {
        bool isRunning = false;

        private Game mGame;
        private Logger mLogger;
        private int kill_count = 0;


        public MainWindow()
        {
            InitializeComponent();
            mLogger = new Logger(this);
            mGame = new Game(this, mLogger);
        }

        private void TestButton_Click(object sender, RoutedEventArgs e)
        {
            ImageAnalyze test = new ImageAnalyze(mLogger);
            bool result = test.FindImageMatchDebug(NodImages.Exit, NodImages.Test, true) != null;
            Console.Out.WriteLine(result);
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (isRunning)
            {
                mLogger.sendMessage("Stopping bot", LogType.INFO);
                start_button.Content = "Start";
                Task.Delay(50).ContinueWith(_ => {
                    isRunning = false;
                    mGame.Stop();
                });
            }
            else
            {
                mLogger.sendMessage("Starting bot", LogType.INFO);
                start_button.Content = "Stop";
                Task.Delay(50).ContinueWith(_ =>
                {
                    isRunning = true;
                    mGame.StartAsync();
                });

            }
            
        }

        /// <summary>
        /// This event handler toggles the Settings based on the events of from the various CheckBoxes in the UI.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            switch (((CheckBox)sender).Name)
            {
                case "pilgrimage":
                    Settings.PILGRIMAGE = true; // Is on a pilgrimage
                    mLogger.sendMessage("PILGRIMAGE ON", LogType.INFO);
                    break;
                case "skip_chests":
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
        /// This event handler toggles the Settings based on the events of from the various CheckBoxes in the UI.
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
        /// </summary>
        public void updateKillCount()
        {
            kill_count++;
            kill_counter_label.Content = kill_count;
        }

        public void updateLog(string aMessage)
        {
            //log_textbox.Text += aMessage;
        }
    }
}
