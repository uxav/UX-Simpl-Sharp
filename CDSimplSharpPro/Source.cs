using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CDSimplSharpPro
{
    public class Source
    {
        public uint ID;
        public string Name;
        public string Icon;
        public string GroupName;
        public eSourceType SourceType;
        public object SourceController;
        public Room Room { get; private set; }

        public Source(uint id)
        {
            this.ID = id;
            this.Name = "Unknown Source";
        }

        public Source(uint id, string name, eSourceType sourceType, string groupName)
        {
            this.ID = id;
            this.Name = name;
            this.SourceType = sourceType;
            this.GroupName = groupName;
        }

        public Source(uint id, string name, eSourceType sourceType, string groupName, object sourceControllerObject)
        {
            this.ID = id;
            this.Name = name;
            this.SourceType = sourceType;
            this.GroupName = groupName;
            this.SourceController = sourceControllerObject;
        }

        public void AssignToRoom(Room room)
        {
            this.Room = room;
        }
    }

    public enum eSourceType
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