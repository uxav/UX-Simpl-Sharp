using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;
using UXLib.UI;

namespace UXLib.Models
{
    public class Room
    {
        public Room(UXSystem system, uint id)
            : this(system, id, null) { }

        public Room(UXSystem system, uint id, Room parentRoom)
        {
            this.System = system;
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

        public UXSystem System { get; private set; }
        public CrestronControlSystem ControlSystem { get { return System.ControlSystem; } }

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
        public bool HasFusion
        {
            get
            {
                if (this.Fusion != null)
                    return true;
                else
                    return false;
            }
        }
        public Fusion Fusion { get; protected set; }

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
                    FusionUpdate();
                }
            }
            get
            {
                return _Source;
            }
        }

        public SourceCollection Sources
        {
            get
            {
                return this.System.Sources.ForRoom(this);
            }
        }

        public UIControllerCollection UserInterfaces
        {
            get
            {
                return this.System.UserInterfaces.ForRoom(this);
            }
        }

        VolumeLevelCollection _VolumeLevels;
        public VolumeLevelCollection VolumeLevels
        {
            get
            {
                if (_VolumeLevels == null)
                    _VolumeLevels = new VolumeLevelCollection();
                return _VolumeLevels;
            }
            protected set
            {
                _VolumeLevels = value;
            }
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

        public virtual void FusionAssign(uint ipId)
        {
            this.Fusion = new Fusion(this, ipId);
        }

        public event RoomDetailsChangeEventHandler RoomDetailsChange;
        public event RoomSourceChangeEventHandler SourceChange;

        public virtual void Initialize()
        {
            
        }

        public virtual void FusionUpdate()
        {
            if (this.Fusion != null)
            {
                this.Fusion.SetSystemPowerStatus((this.Source != null) ? true : false);
            }
        }

        public virtual void FusionSystemPowerRequest(bool powerRequested)
        {
            ErrorLog.Notice("Fusion requested power {0} in room \"{1}\"", (powerRequested) ? "on" : "off", this.Name);
        }

        public virtual void FusionDisplayPowerRequest(bool powerRequested)
        {
            ErrorLog.Notice("Fusion requested displays power {0} in room \"{1}\"", (powerRequested) ? "on" : "off", this.Name);
        }
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
}