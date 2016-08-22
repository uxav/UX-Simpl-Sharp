using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Models;

namespace UXLib.Devices.Audio.QSC
{
    public class QSysControlPair : IVolumeDevice
    {
        public QSysControlPair(string name, QSysControl levelControl, QSysControl muteControl)
        {
            _Name = name;
            if (levelControl.SupportsVolumeLevel)
                QSysLevelControl = levelControl;
            else
                ErrorLog.Error("QSysControlPair Constructor - {0} does not support level", levelControl.Name);
            if (muteControl.SupportsVolumeMute)
                QSysMuteControl = muteControl;
            else
                ErrorLog.Error("QSysControlPair Constructor - {0} does not support mute", muteControl.Name);
        }

        public QSysControl QSysLevelControl { get; protected set; }
        public QSysControl QSysMuteControl { get; protected set; }

        #region IVolumeDevice Members

        string _Name;
        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public bool SupportsVolumeLevel
        {
            get
            {
                if (QSysLevelControl != null) return true;
                return false;
            }
        }

        public bool SupportsVolumeMute
        {
            get
            {
                if (QSysMuteControl != null) return true;
                return false;
            }
        }

        public event VolumeDeviceChangeEventHandler VolumeChanged;

        public ushort VolumeLevel
        {
            get
            {
                if (this.QSysLevelControl != null)
                    return (ushort)UXLib.Tools.ScaleRange(this.QSysLevelControl.ControlPosition, 0, 1, ushort.MinValue, ushort.MaxValue);
                return 0;
            }
            set
            {
                if (this.QSysLevelControl != null)
                    this.QSysLevelControl.ControlPosition = (float)UXLib.Tools.ScaleRange(value, ushort.MinValue, ushort.MaxValue, 0, 1);
            }
        }

        public bool VolumeMute
        {
            get
            {
                if (this.QSysMuteControl != null && this.QSysMuteControl.Value == 1)
                    return true;
                else return false;
            }
            set
            {
                if (this.QSysMuteControl != null)
                {
                    if (value)
                        this.QSysMuteControl.Value = 1;
                    else
                        this.QSysMuteControl.Value = 0;
                }
            }
        }

        #endregion
    }
}