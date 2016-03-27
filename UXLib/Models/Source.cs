using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Models
{
    public class Source
    {
        public Source(uint id, string name, SourceType sourceType)
        {
            this.ID = id;
            this.Name = name;
            this.SourceType = sourceType;
        }

        public Source(uint id, string name, SourceType sourceType, uint inputIndex)
        {
            this.ID = id;
            this.Name = name;
            this.SourceType = sourceType;
            this.InputIndex = inputIndex;
        }

        public Source(uint id, string name, SourceType sourceType, uint inputIndex, string groupName)
            : this(id, name, sourceType, inputIndex)
        {
            this.GroupName = groupName;
        }

        public Source(uint id, string name, SourceType sourceType, uint inputIndex, string groupName, object sourceControllerObject)
            : this(id, name, sourceType, inputIndex, groupName)
        {
            this.SourceController = sourceControllerObject;
        }

        public uint ID { get; protected set; }
        public string Name { get; protected set; }
        public string Icon { get; set; }
        string _GroupName;
        public string GroupName
        {
            get
            {
                if (_GroupName == null || _GroupName == string.Empty)
                    return this.Name;
                else
                    return _GroupName;
            }
            protected set
            {
                _GroupName = value;
            }
        }
        public SourceType SourceType { get; protected set; }
        public uint InputIndex { get; protected set; }
        public object SourceController { get; protected set; }
        public Room Room { get; protected set; }

        public void AssignToRoom(Room room)
        {
            this.Room = room;
        }
    }

    public enum SourceType
    {
        Unknown,
        VideoConference,
        PC,
        Laptop,
        DVD,
        BluRay,
        TV,
        IPTV,
        Satellite,
        Tuner,
        AM,
        FM,
        DAB,
        InternetRadio,
        iPod,
        AirPlay,
        MovieServer,
        MusicServer,
        InternetService,
        AppleTV,
        Chromecast,
        AndroidTV,
        XBox,
        PlayStation,
        NintendoWii,
        AirMedia,
        ClickShare,
        CCTV,
        AuxInput,
        LiveStream
    }
}