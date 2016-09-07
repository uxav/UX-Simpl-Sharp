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

        public CrestronControlSystem ControlSystem { get; private set; }
        public SourceCollection Sources { get; private set; }
        public RoomCollection Rooms { get; private set; }
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