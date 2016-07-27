using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Models
{
    public interface IVolumeDevice
    {
        string Name { get; }
        ushort VolumeLevel { get; set; }
        bool VolumeMute { get; set; }
        bool SupportsVolumeMute { get; }
        bool SupportsVolumeLevel { get; }
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