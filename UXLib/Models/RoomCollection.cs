using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharpPro;

namespace UXLib.Models
{
    public sealed class RoomCollection : UXCollection<Room>
    {
        internal RoomCollection() { }
        internal RoomCollection(Dictionary<uint, Room> fromDictionary)
        {
            foreach (var room in fromDictionary)
            {
                Add(room.Key, room.Value);
            }
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