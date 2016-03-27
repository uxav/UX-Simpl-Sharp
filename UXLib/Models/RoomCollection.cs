using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace UXLib.Models
{
    public class RoomCollection : IEnumerable<Room>
    {
        private List<Room> RoomList;

        public Room this[uint id]
        {
            get
            {
                return this.RoomList.FirstOrDefault(r => r.ID == id);
            }
        }

        public int NumberOfRooms
        {
            get
            {
                return this.RoomList.Count;
            }
        }
        
        public RoomCollection()
            : base()
        {
            this.RoomList = new List<Room>();
        }

        public void Add(Room room)
        {
            this.RoomList.Add(room);
        }

        public void Add(uint id, string name)
        {
            Room newRoom = new Room(id);
            newRoom.Name = name;

            if (!this.RoomList.Exists(r => r.ID == id))
            {
                this.RoomList.Add(newRoom);
            }
        }

        public void Add(uint id, string name, uint parentID)
        {
            if (this.RoomList.Exists(r => r.ID == parentID) && !this.RoomList.Exists(r => r.ID == id))
            {
                Room newRoom = new Room(id, this.RoomList.FirstOrDefault(r => r.ID == parentID));
                newRoom.Name = name;
                this.RoomList.Add(newRoom);
            }
        }

        public IEnumerator<Room> GetEnumerator()
        {
            return this.RoomList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}