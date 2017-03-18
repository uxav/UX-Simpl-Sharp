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
        internal RoomCollection(Dictionary<uint, Room> fromDictionary)
        {
            InternalDictionary = new Dictionary<uint, Room>(fromDictionary);
        }

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

        internal void Remove(Room room)
        {
            if (this.Contains(room))
                InternalDictionary.Remove(room.ID);
        }
    }
}