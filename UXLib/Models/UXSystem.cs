using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        }
    }
}