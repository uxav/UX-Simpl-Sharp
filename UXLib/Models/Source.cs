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

        public Source(uint id, string name, string groupName, SourceType sourceType)
            : this(id, name, sourceType)
        {
            this.GroupName = groupName;
        }

        public uint ID { get; protected set; }
        public string Name { get; protected set; }
        public string Icon { get; set; }
        public uint IconMode { get; set; }
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
        public Room Room { get; protected set; }

        public void AssignToRoom(Room room)
        {
            this.Room = room;
        }

        public bool IsPresentationSource
        {
            get
            {
                switch (this.SourceType)
                {
                    case SourceType.AirMedia:
                    case SourceType.ClickShare:
                    case SourceType.Laptop:
                    case SourceType.PC:
                        return true;
                }
                return false;
            }
        }

        public bool IsWirelessPresentationSource
        {
            get
            {
                switch (this.SourceType)
                {
                    case SourceType.AirMedia:
                    case SourceType.ClickShare:
                        return true;
                }
                return false;
            }
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
        LiveStream,
        SignagePlayer,
        GenericWirelessPresentationDevice
    }
}