using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.CrestronThread;
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
            ChangeGroupID = 0;
            Initialized = false;
            QSys.DataReceived += new QSysReceivedDataEventHandler(QSys_DataReceived);

            if (QSys.Connected)
                Poll();
        }

        internal QSysControl(QSys device, string controlId, QSysControlType type, int changeGroupID)
            : this(device, controlId, type)
        {
            ChangeGroupID = changeGroupID;

            if (QSys.Connected && ChangeGroupID > 0)
            {
                Send(string.Format("cga {0} \"{1}\"", ChangeGroupID, Name));
            }
        }

        /// <summary>
        /// The QSysControlType of the control
        /// </summary>
        public QSysControlType ControlType { get; protected set; }

        /// <summary>
        /// Return true if the control has successfully received a valid value from the device
        /// </summary>
        /// <remarks>False value may indicate a problem with the Control ID</remarks>
        public bool Initialized { get; protected set; }

        internal int ChangeGroupID { get; set; }

        void Send(string stringToSend)
        {
            this.QSys.Send(stringToSend);
        }

        internal void RegisterOnConnect()
        {
            if (this.ChangeGroupID > 0)
            {
                this.Send(string.Format("cga {0} \"{1}\"", ChangeGroupID, this.Name));
            }
            this.Poll();
        }

        /// <summary>
        /// The device owning the control
        /// </summary>
        public QSys QSys { get; protected set; }

        /// <summary>
        /// The Control ID of the control
        /// </summary>
        public string ControlID { get; protected set; }

#if DEBUG
        Stopwatch ThreadTimer;
#endif

        string _StringValue = string.Empty;

        /// <summary>
        /// Set or Get the StringValue of the control
        /// </summary>
        public string StringValue
        {
            get
            {  
                return _StringValue;
            }
            set
            {
                Send(string.Format("css \"{0}\" \"{1}\"", this.ControlID, value));
            }
        }

        float _Value;

        /// <summary>
        /// Set or Get the Value
        /// </summary>
        public float Value
        {
            get
            {
                return _Value;
            }
            set
            {
                Send(string.Format("csv \"{0}\" {1:0.0}", this.ControlID, value));
            }
        }

        float _ControlPosition;

        /// <summary>
        /// Set or Get the Control Position of the control
        /// </summary>
        /// <remarks>This is a float from 0 to 1</remarks>
        public float ControlPosition
        {
            get
            {
                return _ControlPosition;
            }
            set
            {
                if (value >= 0 && value <= 1)
                    Send(string.Format("csp \"{0}\" {1}", this.ControlID, value));
                else
                    ErrorLog.Error("Invalid value for {0}.ControlPosition", this.ControlID);
            }
        }

        /// <summary>
        /// To trigger a trigger-type control such as the "Play" button in the Audio Player.
        /// </summary>
        public void Trigger()
        {
            Send(string.Format("ct \"{0}\"", this.ControlID));
        }

        /// <summary>
        /// Poll the control
        /// </summary>
        public void Poll()
        {
            this.QSys.Send(string.Format("cg \"{0}\"", this.ControlID));
        }

        /// <summary>
        /// Poll the control syncronously
        /// </summary>
        /// <remarks>This is a blocking call to ensure the control gets the current values</remarks>
        public void PollSync()
        {
#if DEBUG
            ThreadTimer = new Stopwatch();
            ThreadTimer.Start();
            CrestronConsole.PrintLine("QSys Control \"{0}\" Poll - {1}", this.Name, this.ThreadTimer.Elapsed);
#endif
            if (QSys.Connected)
            {
                Thread pollThread = new Thread(PollThreadProcess, null, Thread.eThreadStartOptions.CreateSuspended);
                pollThread.Priority = Thread.eThreadPriority.HighPriority;
                pollThread.Start();
                pollThread.Join();
            }
#if DEBUG
            CrestronConsole.PrintLine("QSys Control \"{0}\" Poll End - {1}", this.Name, this.ThreadTimer.Elapsed);
            ThreadTimer.Stop();
#endif
        }

        bool Waiting = false;

        object PollThreadProcess(object o)
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();
#if DEBUG
            CrestronConsole.PrintLine("Started poll thread on Control: {0}, {1}", this.Name, ThreadTimer.Elapsed);
#endif
            Waiting = true;
#if DEBUG
            CrestronConsole.PrintLine("Polling... {0}", ThreadTimer.Elapsed);
#endif
            this.Poll();
#if DEBUG
            CrestronConsole.PrintLine("Waiting = {0}, {1}", Waiting, ThreadTimer.Elapsed);
#endif
            while (Waiting)
            {
                Thread.Sleep(1);

                if (timer.ElapsedMilliseconds >= 1000)
                {
#if DEBUG
                    CrestronConsole.PrintLine("Waiting timedout");
#endif
                    Waiting = false;
                }
            }
            return null;
        }

        void QSys_DataReceived(QSys device, QSysReceivedDataEventArgs args)
        {
            if (args.ResponseType == "cv" && args.Arguments.First() == this.ControlID)
            {
#if DEBUG
                CrestronConsole.PrintLine("{0} Value = {1}", args.Arguments[0], args.Arguments[1]);
#endif

                try
                {
                    _StringValue = args.Arguments[1];
                    _ControlPosition = float.Parse(args.Arguments[3]);
                    float newValue = float.Parse(args.Arguments[2]);

                    if (_Value != newValue)
                    {
                        _Value = newValue;

                        if (VolumeChanged != null)
                        {
                            if (this.SupportsVolumeLevel)
                                VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.LevelChanged));
                            else if (this.SupportsVolumeMute)
                                VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.MuteChanged));
                        }
                    }

                    if (ValueChanged != null)
                        ValueChanged(this);

                    if (Waiting)
                    {
                        Waiting = false;
                    }

                    if (this.Initialized == false)
                        this.Initialized = true;
                }
                catch
                {
                    ErrorLog.Error("Error in QSysControl Rx: \"{0}\", args Count = {1}", args.DataString, args.Arguments.Count);
                }
            }
        }

        public event QSysControlChangeEventHandler ValueChanged;

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

    public delegate void QSysControlChangeEventHandler(QSysControl control);

    public enum QSysControlType
    {
        Level,
        Mute,
        Other
    }
}