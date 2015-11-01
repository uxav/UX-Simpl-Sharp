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

namespace CDSimplSharpPro.SystemConfig
{
    public class Config
    {
        public Dictionary<uint, UserInterface> Interfaces;
        public bool Loaded;

        public Config(CrestronControlSystem controlSystem, Rooms rooms, UserInterfaces userInterfaces, string configFilePath)
        {
            // Init properties
            this.Loaded = false;
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
                    ConfigData data = JsonConvert.DeserializeObject<ConfigData>(configString);
                    CrestronConsole.PrintLine("   System Type: {0}", data.system_type);

                    List<RoomData> configRooms = data.rooms.OrderBy(o => o.id).ToList();

                    foreach (var room in configRooms)
                    {
                        if (room.parent_id == 0)
                        {
                            rooms.Add(room.id, room.name);
                        }
                        else
                        {
                            rooms.Add(room.id, room.name, room.parent_id);
                        }

                        CrestronConsole.PrintLine("   Room ID {0}, {1}", room.id, room.name);
                    }

                    foreach (UIData ui in data.interfaces)
                    {
                        userInterfaces.Add(controlSystem, ui.id, ui.ip_id, ui.type, ui.name, rooms[ui.default_room]);
                        CrestronConsole.PrintLine("   User Interface ({0} - {1}) with ID {2}, {3}, assigned to Room: {4}",
                            userInterfaces[ui.id].Device.GetType(), userInterfaces[ui.id].Device.ID, ui.id, ui.name, rooms[ui.default_room].Name);
                    }

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