using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CDSimplSharpPro
{
    public class Source
    {
        public uint ID;
        public string Name;
        public string Icon;
        public object SourceController;
        public Room Room { get; private set; }

        public Source(uint id)
        {
            this.ID = id;
            this.Name = "Unknown Source";
        }

        public Source(uint id, string name)
        {
            this.ID = id;
            this.Name = name;
        }

        public Source(uint id, string name, object sourceControllerObject)
        {
            this.ID = id;
            this.Name = name;
            this.SourceController = sourceControllerObject;
        }

        public void AssignToRoom(Room room)
        {
            this.Room = room;
        }
    }
}