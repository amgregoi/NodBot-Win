﻿using Microsoft.Win32;
using NodBot.Code;
using NodBot.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;
using Newtonsoft.Json;

namespace NodBot
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class NodBotInit : Window
    {

        private NodBotAI player = new NodBotAI();
        private bool playerLoaded = false;
        private bool playerNeutralSet = false;

        public NodBotInit()
        {
            InitializeComponent();
        }

        private void LoadButton_Click(object sender, RoutedEventArgs e)
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
                }
            }
            catch (Exception ex)
            {
                Console.Out.WriteLine(ex.Message);
                Console.Out.WriteLine(ex.StackTrace);
            }
        }

        private void NeutralButton_Click(object sender, RoutedEventArgs e)
        {
            ImageAnalyze test = new ImageAnalyze();
            ImageAnalyze.CaputreNeutralPoint();
            playerNeutralSet = true;
        }

        private void DoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (playerLoaded && playerNeutralSet)
            {
                player.Show();
                this.Close();
            }
            else
            {
                // TODO :: display requirement that hasn't been met.
            }
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
            label_profile.Content = "Loaded: " + settings.playerName;
            playerLoaded = true;
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