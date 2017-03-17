using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.CrestronThread;
using Crestron.SimplSharpPro.Fusion;
using UXLib.Models.Fusion;
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
            this.System.Rooms.Add(this);
            this.ParentRoom = parentRoom;
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
                if (_Name.Length == 0)
                    return string.Format("{0} {1}", this.GetType().Name, this.ID);
                return _Name;
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

        Room _ParentRoom;
        public Room ParentRoom
        {
            get
            {
                return _ParentRoom;
            }
            set
            {
                _ParentRoom = value;
            }
        }

        public Room MasterRoom
        {
            get
            {
                Room room = this;
                while (room.IsChild)
                    room = room.ParentRoom;
                return room;
            }
        }

        public RoomCollection Children
        {
            get
            {
                Dictionary<uint, Room> rooms = new Dictionary<uint, Room>();
                foreach (Room room in this.System.Rooms.ToList().Where(r => r.ParentRoom == this))
                {
                    rooms.Add(room.ID, room);
                }
                return new RoomCollection(rooms);
            }
        }

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
        public FusionController Fusion { get; protected set; }

        public virtual void OnSourceChange(Source previousSource, Source newSource)
        {
            try
            {
                if (this.SourceChange != null)
                    this.SourceChange(this, new RoomSourceChangeEventArgs(previousSource, newSource));
            }
            catch (Exception e)
            {
                ErrorLog.Exception(string.Format("Error calling {0}.SourceChange Event", this.GetType().Name), e);
            }
        }

        Source _Source;
        public Source Source
        {
            set
            {
                if (_Source != value && (SourceChangeThread == null || SourceChangeThread.ThreadState == Thread.eThreadStates.ThreadFinished))
                {
                    SourceChangeThread = new Thread(SourceChangeProcess, value);
                }
            }
            get
            {
                return _Source;
            }
        }

        Thread SourceChangeThread;
        object SourceChangeProcess(object value)
        {
            Source oldSource = _Source;

            if (value != null)
            {
                _Source = (Source)value;
#if DEBUG
                CrestronConsole.PrintLine("Room {0}, {1} has switched to source: {2}", this.ID, this.Name, _Source.Name);
                ErrorLog.Notice("Room {0}, {1} has switched to source: {2}", this.ID, this.Name, _Source.Name);
#endif
            }
            else
            {
                _Source = null;
#if DEBUG
                CrestronConsole.PrintLine("Room {0}, {1} now has no source selected", this.ID, this.Name);
                ErrorLog.Notice("Room {0}, {1} now has no source selected", this.ID, this.Name);
#endif
            }
            OnSourceChange(oldSource, _Source);
            FusionUpdate();

            return null;
        }

        public virtual SourceCollection Sources
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
                return (this.Children.Count > 0);
            }
        }

        public bool IsChild
        {
            get
            {
                return (this.ParentRoom != null);
            }
        }

        public bool IsMaster
        {
            get
            {
                return (this.IsParent && !this.IsChild);
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
            this.Fusion = new FusionController(this, ipId, false);
        }

        public virtual void FusionAssign(uint ipId, bool useScheduling)
        {
            this.Fusion = new FusionController(this, ipId, useScheduling);
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

        public virtual void Shutdown()
        {

        }

        public override string ToString()
        {
            return string.Format("Room ID: {0} \"{1}\"", this.ID, this.Name);
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