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
        public Dictionary<uint, Room> Rooms;
        public Dictionary<uint, UserInterface> Interfaces;
        public bool Loaded;

        public Config(CrestronControlSystem controlSystem, string configFilePath)
        {
            // Init properties
            this.Loaded = false;
            this.Rooms = new Dictionary<uint, Room>();
            this.Interfaces = new Dictionary<uint, UserInterface>();
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

                    List<RoomData> rooms = data.rooms.OrderBy(o => o.id).ToList();

                    foreach (var room in rooms)
                    {
                        Room newRoom;

                        if (room.parent_id == 0)
                        {
                            newRoom = new Room(room.id);
                        }
                        else if (Rooms.ContainsKey(room.parent_id))
                        {
                            newRoom = new Room(room.id, Rooms[room.parent_id]);
                        }
                        else
                        {
                            newRoom = null;
                            ErrorLog.Error("Adding room id: {0} was not done due to an issue with the parent id: {1}", room.id, room.parent_id);
                        }

                        if (newRoom != null)
                        {
                            newRoom.Name = room.name;
                            newRoom.Location = room.location;
                            this.Rooms.Add(newRoom.ID, newRoom);
                            CrestronConsole.PrintLine("   Room ID {0}, {1}", newRoom.ID, newRoom.Name);
                        }
                    }

                    foreach (UIData ui in data.interfaces)
                    {
                        UserInterface newInterface = new UserInterface(controlSystem, ui.id, ui.ip_id, ui.type, Rooms[ui.default_room]);
                        newInterface.Name = ui.name;
                        if (newInterface.Register() != Crestron.SimplSharpPro.eDeviceRegistrationUnRegistrationResponse.Success)
                        {
                            ErrorLog.Error("Could not register User Interface with ID: {0}, ipID: {1}", ui.id, ui.ip_id);
                        }
                        else
                        {
                            Interfaces.Add(newInterface.ID, newInterface);
                            CrestronConsole.PrintLine("   User Interface ({0} - {1}) with ID {2}, {3}, assigned to Room: {4}",
                                newInterface.Device.GetType(), newInterface.Device.ID, newInterface.ID, newInterface.Name, newInterface.Room.Name);
                        }
                    }
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