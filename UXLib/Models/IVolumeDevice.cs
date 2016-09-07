using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Models
{
    public interface IVolumeDevice
    {
        /// <summary>
        /// The name of the device
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Set or Get the volume level
        /// </summary>
        ushort VolumeLevel { get; set; }

        /// <summary>
        /// Set or Get the mute
        /// </summary>
        bool VolumeMute { get; set; }

        /// <summary>
        /// Return true if this supports Mute control
        /// </summary>
        bool SupportsVolumeMute { get; }

        /// <summary>
        /// Returns true id this supports Level control
        /// </summary>
        bool SupportsVolumeLevel { get; }

        /// <summary>
        /// Event raised when a level or mute status changes
        /// </summary>
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