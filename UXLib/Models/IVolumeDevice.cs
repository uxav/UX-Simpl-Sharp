using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Models
{
    public interface IVolumeDevice
    {
        ushort Level { get; set; }
        bool Mute { get; set; }
        bool SupportsMute { get; }
        bool SupportsLevel { get; }
        event VolumeDeviceChangeEventHandler VolumeChanged;
    }

    public delegate void VolumeDeviceChangeEventHandler(IVolumeDevice device, VolumeChangeEventArgs args);

    public class VolumeChangeEventArgs : EventArgs
    {
        public VolumeChangeEventArgs(VolumeLevelChangeEventType eventType)
        {
            EventType = eventType;
        }

        public VolumeLevelChangeEventType EventType;
    }

    public enum VolumeLevelChangeEventType
    {
        LevelChanged,
        MuteChanged
    }
}