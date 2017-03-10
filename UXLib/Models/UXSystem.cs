using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using UXLib.UI;

namespace UXLib.Models
{
    public class UXSystem
    {
        public UXSystem(CrestronControlSystem controlSystem)
        {
            ControlSystem = controlSystem;
            this.Rooms = new RoomCollection();
            this.Sources = new SourceCollection();
            this.UserInterfaces = new UIControllerCollection();
        }

        /// <summary>
        /// Get the CrestronControlSystem for the system
        /// </summary>
        public CrestronControlSystem ControlSystem { get; private set; }

        /// <summary>
        /// A collection of sources for the system
        /// </summary>
        public SourceCollection Sources { get; private set; }

        /// <summary>
        /// A collection of rooms for the system
        /// </summary>
        public RoomCollection Rooms { get; private set; }

        /// <summary>
        /// A collection of UIControllers for the system
        /// </summary>
        public UIControllerCollection UserInterfaces { get; private set; }

        public virtual void Initialize()
        {
            foreach (Room room in this.Rooms)
            {
                room.Initialize();
            }

            CrestronConsole.AddNewConsoleCommand(ConsoleGetRoomName,
                "GetRoomName", "Get room name for Room ID", ConsoleAccessLevelEnum.AccessOperator);
        }

        void ConsoleGetRoomName(string argsString)
        {
            try
            {
                string[] args = argsString.Split(',');
                CrestronConsole.ConsoleCommandResponse("Room name for ID {0} is {1}",
                    this.Rooms[uint.Parse(args[0])].ID,
                    this.Rooms[uint.Parse(args[0])].Name);
            }
            catch (Exception e)
            {
                CrestronConsole.ConsoleCommandResponse("Error {0}", e.Message);
            }
        }
    }
}