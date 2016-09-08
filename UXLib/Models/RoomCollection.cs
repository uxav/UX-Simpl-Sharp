﻿using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharpPro;

namespace UXLib.Models
{
    public class RoomCollection : IEnumerable<Room>
    {
        internal RoomCollection()
        {
            this.RoomList = new List<Room>();
        }
        
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

        public void Add(Room room)
        {
            this.RoomList.Add(room);
        }

        public void Add(UXSystem system, uint id, string name)
        {
            Room newRoom = new Room(system, id);
            newRoom.Name = name;

            if (!this.RoomList.Exists(r => r.ID == id))
            {
                this.RoomList.Add(newRoom);
            }
        }

        public void Add(UXSystem system, uint id, string name, uint parentID)
        {
            if (this.RoomList.Exists(r => r.ID == parentID) && !this.RoomList.Exists(r => r.ID == id))
            {
                Room newRoom = new Room(system, id, this.RoomList.FirstOrDefault(r => r.ID == parentID));
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