using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;

namespace UXLib.Models
{
    public class Room
    {
        public Room(uint id)
            : this(id, null) { }

        public Room(uint id, Room parentRoom)
        {
            // Room created with parent!
            this.ID = id;
            this._Name = "";
            this._Location = "";
            if (parentRoom != null)
            {
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
        }

        /// <summary>
        /// This is the unique ID of the room. Try to use an ordered index
        /// </summary>
        public uint ID { get; private set; }
        
        private string _Name;
        
        /// <summary>
        /// The Name of the room. Changing this will invoke the <see cref="RoomDetailsChange"/> event.
        /// </summary>
        public string Name
        {
            get
            {
                return this._Name;
            }
            set
            {
                this._Name = value;
                
                if(this.RoomDetailsChange != null)
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
                
                if(this.RoomDetailsChange != null)
                    this.RoomDetailsChange(this, new RoomDetailsChangeEventArgs());
            }
        }
        public Room ParentRoom { get; private set; }
        public Room MasterRoom { get; private set; }
        public Room ChildRoom { get; private set; }
        public FusionRoom FusionRoom;

        public virtual void OnSourceChange(Source previousSource, Source newSource)
        {
            if (SourceChange != null)
            {
                this.SourceChange(this, new RoomSourceChangeEventArgs(previousSource, newSource));
            }
        }

        Source _Source;
        public Source Source
        {
            set
            {
                if (_Source != value)
                {
                    Source oldSource = _Source;
                    _Source = value;
#if DEBUG
                    if (value != null)
                    {
                        CrestronConsole.PrintLine("Room {0}, {1} has switched to source: {2}", this.ID, this.Name, _Source.Name);
                        ErrorLog.Notice("Room {0}, {1} has switched to source: {2}", this.ID, this.Name, _Source.Name);
                    }
                    else
                    {
                        CrestronConsole.PrintLine("Room {0}, {1} now has no source selected", this.ID, this.Name);
                        ErrorLog.Notice("Room {0}, {1} now has no source selected", this.ID, this.Name);
                    }
#endif
                    OnSourceChange(oldSource, _Source);
                }
            }
            get
            {
                return _Source;
            }
        }

        ushort _VolumeSource;
        public virtual ushort VolumeSource
        {
            get
            {
                return _VolumeSource;
            }
            set
            {
                if (_VolumeSource != value)
                {
                    _VolumeSource = value;
                    OnVolumeChange(RoomVolumeType.Source, _VolumeSource);
                }
            }
        }

        ushort _VolumeVC;
        public virtual ushort VolumeVC
        {
            get
            {
                return _VolumeVC;
            }
            set
            {
                if (_VolumeVC != value)
                {
                    _VolumeVC = value;
                    OnVolumeChange(RoomVolumeType.VideoConference, _VolumeVC);
                }
            }
        }

        ushort _VolumeAC;
        public virtual ushort VolumeAC
        {
            get
            {
                return _VolumeAC;
            }
            set
            {
                if (_VolumeAC != value)
                {
                    _VolumeAC = value;
                    OnVolumeChange(RoomVolumeType.AudioConference, _VolumeAC);
                }
            }
        }

        public event RoomVolumeChangeEventHandler VolumeChanged;

        protected virtual void OnVolumeChange(RoomVolumeType type, ushort levelValue)
        {
            if (VolumeChanged != null)
                VolumeChanged(this, new RoomVolumeChangeEventArgs(type, levelValue));
        }

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

        public void FusionRegister(uint ipId, CrestronControlSystem controlSystem)
        {
            this.FusionRoom = new FusionRoom(ipId, controlSystem, this.Name, Guid.NewGuid().ToString());

            if (this.FusionRoom.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
            {
                ErrorLog.Error("Room ID: {0}, {1}, Could not register a Fusion Room instance", this.ID, this.Name);
            }
            else
            {
                this.FusionRoom.FusionStateChange += new FusionStateEventHandler(FusionRoom_FusionStateChange);
                this.FusionRoom.FusionAssetStateChange += new FusionAssetStateEventHandler(FusionRoom_FusionAssetStateChange);
                this.FusionRoom.OnlineStatusChange += new OnlineStatusChangeEventHandler(FusionRoom_OnlineStatusChange);
            }
        }

        void FusionRoom_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            
        }

        void FusionRoom_FusionAssetStateChange(FusionBase device, FusionAssetStateEventArgs args)
        {
            
        }

        void FusionRoom_FusionStateChange(FusionBase device, FusionStateEventArgs args)
        {
            
        }

        public event RoomDetailsChangeEventHandler RoomDetailsChange;
        public event RoomSourceChangeEventHandler SourceChange;
    }

    public delegate void RoomDetailsChangeEventHandler(Room room, RoomDetailsChangeEventArgs args);
    public delegate void RoomSourceChangeEventHandler(Room room, RoomSourceChangeEventArgs args);

    public class RoomDetailsChangeEventArgs : EventArgs
    {
        public RoomDetailsChangeEventArgs()
            : base()
        {

        }
    }

    public class RoomSourceChangeEventArgs : EventArgs
    {
        public Source PreviousSource;
        public Source NewSource;
        public RoomSourceChangeEventArgs(Source previousSource, Source newSource)
            : base()
        {
            this.PreviousSource = previousSource;
            this.NewSource = newSource;
        }
    }

    public enum RoomVolumeType
    {
        Source,
        VideoConference,
        AudioConference
    }

    public delegate void RoomVolumeChangeEventHandler(Room room, RoomVolumeChangeEventArgs args);

    public class RoomVolumeChangeEventArgs : EventArgs
    {
        public RoomVolumeChangeEventArgs(RoomVolumeType type, ushort levelValue)
        {
            VolumeType = type;
            LevelValue = levelValue;
        }

        public RoomVolumeType VolumeType;
        public ushort LevelValue;
    }
}