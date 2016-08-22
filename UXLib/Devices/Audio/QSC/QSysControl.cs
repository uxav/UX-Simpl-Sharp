using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Models;

namespace UXLib.Devices.Audio.QSC
{
    public class QSysControl : IVolumeDevice
    {
        internal QSysControl(QSys device, string controlId, QSysControlType type)
        {
            QSys = device;
            ControlID = controlId;
            ControlType = type;
            QSys.HasConnected += new QSysConnectedEventHandler(QSys_HasConnected);
            QSys.DataReceived += new QSysReceivedDataEventHandler(QSys_DataReceived);

            if (QSys.Connected)
                this.Poll();
        }

        public QSysControlType ControlType { get; protected set; }

        void QSys_HasConnected(QSys device)
        {
            new CTimer(Poll, null, 10, 2000);
        }

        public QSys QSys { get; protected set; }
        public string ControlID { get; protected set; }

        string _StringValue = string.Empty;
        public string StringValue
        {
            get { return _StringValue; }
        }

        float _Value;
        public float Value
        {
            get
            {
                return _Value;
            }
            set
            {
                this.QSys.Send(string.Format("csv \"{0}\" {1:0.0}", this.ControlID, value));
            }
        }

        float _ControlPosition;
        public float ControlPosition
        {
            get
            {
                return _ControlPosition;
            }
            set
            {
                if (value >= 0 && value <= 1)
                    this.QSys.Send(string.Format("csp \"{0}\" {1}", this.ControlID, value));
                else
                    ErrorLog.Error("Invalid value for {0}.ControlPosition", this.ControlID);
            }
        }

        void Poll(object o)
        {
            this.Poll();
        }

        public void Poll()
        {
            this.QSys.Send(string.Format("cg \"{0}\"", this.ControlID));
        }

        void QSys_DataReceived(QSys device, QSysReceivedDataEventArgs args)
        {
            if (args.ResponseType == "cv" && args.Arguments.First() == this.ControlID)
            {
#if DEBUG
                CrestronConsole.PrintLine("{0} Value = {1}", args.Arguments[0], args.Arguments[1]);
#endif

                if (_Value != float.Parse(args.Arguments[2]) && VolumeChanged != null)
                {
                    if (this.SupportsVolumeLevel)
                        VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.LevelChanged));
                    else if (this.SupportsVolumeMute)
                        VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.MuteChanged));
                }

                _StringValue = args.Arguments[1];
                _Value = float.Parse(args.Arguments[2]);
                _ControlPosition = float.Parse(args.Arguments[3]);
            }
        }

        #region IVolumeDevice Members

        public string Name
        {
            get { return this.ControlID; }
        }

        public bool SupportsVolumeLevel
        {
            get
            {
                if (this.ControlType == QSysControlType.Level) return true;
                else return false;
            }
        }

        public bool SupportsVolumeMute
        {
            get
            {
                if (this.ControlType == QSysControlType.Mute) return true;
                else return false;
            }
        }

        public event VolumeDeviceChangeEventHandler VolumeChanged;

        public ushort VolumeLevel
        {
            get
            {
                return (ushort)UXLib.Tools.ScaleRange(this.ControlPosition, 0, 1, ushort.MinValue, ushort.MaxValue);
            }
            set
            {
                this.ControlPosition = (float)UXLib.Tools.ScaleRange(value, ushort.MinValue, ushort.MaxValue, 0, 1);
            }
        }

        public bool VolumeMute
        {
            get
            {
                if (this.Value == 1)
                    return true;
                else
                    return false;
            }
            set
            {
                if (value)
                    this.Value = 1;
                else
                    this.Value = 0;
            }
        }

        #endregion
    }

    public enum QSysControlType
    {
        Level,
        Mute,
        Other
    }
}