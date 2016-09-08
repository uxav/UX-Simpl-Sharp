using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharpPro;

namespace UXLib.Models
{
    public class RoomCollection : UXCollection<Room>
    {
        internal RoomCollection() { }

        public override Room this[uint roomID]
        {
            get
            {
                return base[roomID];
            }
            internal set
            {
                base[roomID] = value;
            }
        }

        public override bool Contains(uint roomID)
        {
            return base.Contains(roomID);
        }

        public void Add(Room room)
        {
            this[room.ID] = room;
        }

        public void Add(UXSystem system, uint roomID, string roomName)
        {
            Room newRoom = new Room(system, roomID);
            newRoom.Name = roomName;

            this[newRoom.ID] = newRoom;
        }

        public void Add(UXSystem system, uint id, string name, uint parentID)
        {
            if (this.Contains(parentID))
            {
                Room newRoom = new Room(system, id, this[parentID]);
                newRoom.Name = name;
                this[newRoom.ID] = newRoom;
            }
            else
            {
                throw new IndexOutOfRangeException(string.Format("Cannot add room with parent as parentID {0} does not exist", parentID));
            }
        }
    }
}