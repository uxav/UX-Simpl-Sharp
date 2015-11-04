using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Newtonsoft.Json;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.UI;
using Crestron.SimplSharpPro.DeviceSupport;
using CDSimplSharpPro.UI;

namespace CDSimplSharpPro.SystemConfig
{
    public class Config
    {
        public bool Loaded;
        public ConfigData Data;

        public Config(string configFilePath)
        {
            // Init properties
            this.Loaded = false;

            // string for file contents
            string configString = "";

            // Load config file if it exists
            if (File.Exists(configFilePath))
            {
                StreamReader file = new StreamReader(configFilePath);
                configString = file.ReadToEnd();
                file.Close();

                CrestronConsole.PrintLine("Config file loaded with {0} bytes.... reading", configString.Length);
                CrestronConsole.PrintLine("Setting up system with the following information:");

                try
                {
                    this.Data = JsonConvert.DeserializeObject<ConfigData>(configString);
                    this.Loaded = true;
                }
                catch (Exception e)
                {
                    ErrorLog.Error("Could not deserialize json data to object type<ConfigData>: {0}", e.Message);
                }
            }
            else
            {
                ErrorLog.Error("Config file missing and could not be found at {0}", configFilePath);
            }
        }
    }
}