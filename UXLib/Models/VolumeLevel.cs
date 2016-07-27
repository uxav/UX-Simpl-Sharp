using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Models
{
    public class VolumeLevel
    {
        public VolumeLevel(Room room, VolumeLevelType levelType, IVolumeDevice volumeDevice)
        {
            this.Room = room;
            this.Device = volumeDevice;
            this.LevelType = levelType;
            volumeDevice.VolumeChanged += new VolumeDeviceChangeEventHandler(volumeDevice_VolumeChanged);

#if DEBUG
            CrestronConsole.PrintLine("Created new VolumeLevel for Room {0}, {1}, {2}", room.Name, levelType.ToString(), volumeDevice.Name);
#endif
        }

        public VolumeLevelType LevelType { get; protected set; }

        public ushort Level
        {
            get
            {
                return this.Device.VolumeLevel;
            }
            set
            {
                this.Device.VolumeLevel = value;
            }
        }

        public bool Mute
        {
            get
            {
                return this.Device.VolumeMute;
            }
            set
            {
                this.Device.VolumeMute = value;
            }
        }

        public Room Room { get; protected set; }
        public IVolumeDevice Device { get; protected set; }
        public event VolumeLevelChangeEventHandler VolumeChanged;

        void volumeDevice_VolumeChanged(IVolumeDevice device, VolumeChangeEventArgs args)
        {
            if (VolumeChanged != null)
                VolumeChanged(this, args);
        }
    }

    public delegate void VolumeLevelChangeEventHandler(VolumeLevel volumeLevel, VolumeChangeEventArgs args);

    public enum VolumeLevelType
    {
        Source,
        VideoConference,
        AudioConference,
        RecordMix,
        Microphone
    }
}