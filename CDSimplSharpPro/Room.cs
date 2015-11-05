using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;

namespace CDSimplSharpPro
{
    public class Room
    {
        public uint ID { get; private set; }
        private string _Name;
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
                this.RoomDetailsChange(this, new RoomDetailsChangeEventArgs());
            }
        }
        private string _Location;
        public string Location
        {
            get
            {
                return this._Location;
            }
            set
            {
                this._Location = value;
                this.RoomDetailsChange(this, new RoomDetailsChangeEventArgs());
            }
        }
        public Room ParentRoom { get; private set; }
        public Room MasterRoom { get; private set; }
        public Room ChildRoom { get; private set; }
        public FusionRoom FusionRoom;

        public bool IsParent
        {
            get
            {
                if (this.ChildRoom != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsChild
        {
            get
            {
                if (this.ParentRoom != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool HasChild
        {
            get
            {
                if (this.ChildRoom != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public bool IsMaster
        {
            get
            {
                if (this.ParentRoom != null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public int ParentRoomCount
        {
            get
            {
                Room tempRoom = this;
                int result = 0;

                while (!tempRoom.IsMaster)
                {
                    result++;
                    tempRoom = tempRoom.ParentRoom;
                }

                return result;
            }
        }

        public Room(uint id)
        {
            // Room created with no parent so give it the id and init other properties.
            this.ID = id;
            this._Name = "";
            this._Location = "";
            this.ParentRoom = null;
            this.MasterRoom = null;
            this.ChildRoom = null;
        }

        public Room(uint id, Room parentRoom)
        {
            // Room created with parent!
            this.ID = id;
            this._Name = "";
            this._Location = "";
            this.ParentRoom = parentRoom;
            this.ParentRoom.ChildRoom = this;

            // Find the master room by looping through the parents.
            Room room = parentRoom;
            while (room.IsChild)
            {
                room = room.ParentRoom;
            }

            // This room shouldn't have a parent (isn't a child) and is the master room.
            this.MasterRoom = room;
        }

        public void FusionRegister(uint ipId, CrestronControlSystem controlSystem)
        {
            this.FusionRoom = new FusionRoom(ipId, controlSystem, this.Name, Guid.NewGuid().ToString());

            if (this.FusionRoom.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
            {
                ErrorLog.Error("Room ID: {0}, {1}, Could not register a Fusion Room instance", this.ID, this.Name);
            }
        }

        public event RoomDetailsChangeEventHandler RoomDetailsChange;
    }

    public delegate void RoomDetailsChangeEventHandler(Room room, RoomDetailsChangeEventArgs args);

    public class RoomDetailsChangeEventArgs : EventArgs
    {
        public RoomDetailsChangeEventArgs()
            : base()
        {

        }
    }
}