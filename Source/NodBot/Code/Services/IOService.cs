using Newtonsoft.Json;
using NodBot.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodBot.Code.Services
{
    class IOService
    {



        public void readSettingFile()
        {
            string[] lines = System.IO.File.ReadAllLines(Settings.SETTINGS_FILE);

            string json = File.ReadAllText(Settings.SETTINGS_FILE);
            PlayerSettings settings = Deserialize<PlayerSettings>(json);

            if (settings.playerName == null || settings.playerName.Length == 0)
            {
                // Player Name required
                return;
            }

            Settings.Player = settings;
            Settings.WINDOW_NAME = settings.playerName;
        }


        public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);
        }

        public String Serialize<T>(T settings)
        {
            return JsonConvert.SerializeObject(settings);
        }
    }
}
