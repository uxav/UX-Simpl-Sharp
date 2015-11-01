using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace CDSimplSharpPro
{
    public class Rooms : Dictionary<uint, Room>
    {
        public Rooms()
            : base()
        {
            
        }

        public void Add(Room room)
        {
            base.Add(room.ID, room);
        }

        public void Add(uint id, string name)
        {
            Room newRoom = new Room(id);
            newRoom.Name = name;

            if (!this.ContainsKey(id))
            {
                base.Add(newRoom.ID, newRoom);
            }
        }

        public void Add(uint id, string name, uint parentID)
        {
            if (this.ContainsKey(parentID) && !this.ContainsKey(id))
            {
                Room newRoom = new Room(id, this[parentID]);
                newRoom.Name = name;
                base.Add(newRoom.ID, newRoom);
            }
        }
    }
}