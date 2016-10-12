using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.QSC
{
    /// <summary>
    /// A softphone element of a QSys Core
    /// </summary>
    public class QSysSoftPhone
    {
        internal QSysSoftPhone(QSys device, int id, string idOffHookLED, string idRingingLED, string idConnect, string idDisconnect,
            string idDialString, string idDND, string idProgress, string idKeypadBaseName, int changeGroupID)
        {
            this.QSys = device;
            KeypadControls = new Dictionary<string, QSysControl>();
            OffHookLEDControl = this.QSys.Controls.Register(idOffHookLED, QSysControlType.Other, changeGroupID);
            ConnectControl = this.QSys.Controls.Register(idConnect, QSysControlType.Other);
            DisconnectControl = this.QSys.Controls.Register(idDisconnect, QSysControlType.Other);
            DialStringControl = this.QSys.Controls.Register(idDialString, QSysControlType.Other, changeGroupID);
            RingingLEDControl = this.QSys.Controls.Register(idRingingLED, QSysControlType.Other, changeGroupID);
            DNDControl = this.QSys.Controls.Register(idDND, QSysControlType.Other, changeGroupID);
            ProgressControl = this.QSys.Controls.Register(idProgress, QSysControlType.Other, changeGroupID);

            for (int digit = 0; digit <= 9; digit++)
            {
                KeypadControls.Add(digit.ToString(), this.QSys.Controls.Register(
                    string.Format("{0}{1}", idKeypadBaseName, digit.ToString()), QSysControlType.Other));
            }

            KeypadControls.Add("*", this.QSys.Controls.Register(string.Format("{0}*", idKeypadBaseName), QSysControlType.Other));
            KeypadControls.Add("#", this.QSys.Controls.Register(string.Format("{0}#", idKeypadBaseName), QSysControlType.Other));

            OffHookLEDControl.ValueChanged += new QSysControlChangeEventHandler(OnValueChange);
            DialStringControl.ValueChanged += new QSysControlChangeEventHandler(OnValueChange);
            RingingLEDControl.ValueChanged += new QSysControlChangeEventHandler(OnValueChange);
            DNDControl.ValueChanged += new QSysControlChangeEventHandler(OnValueChange);
            ProgressControl.ValueChanged += new QSysControlChangeEventHandler(OnValueChange);
        }

        /// <summary>
        /// The QSys device owning the control elements
        /// </summary>
        public QSys QSys { get; protected set; }

        /// <summary>
        /// The internal id generated to track the phone instance
        /// </summary>
        public int ID { get; protected set; }

        QSysControl OffHookLEDControl { set; get; }
        QSysControl ConnectControl { set; get; }
        QSysControl DisconnectControl { set; get; }
        QSysControl DialStringControl { set; get; }
        QSysControl RingingLEDControl { set; get; }
        QSysControl DNDControl { set; get; }
        QSysControl ProgressControl { set; get; }

        Dictionary<string, QSysControl> KeypadControls { set; get; }

        /// <summary>
        /// Geth the off hook status of the phone
        /// </summary>
        public bool OffHook
        {
            get
            {
                return (OffHookLEDControl.ControlPosition == 1);
            }
        }

        /// <summary>
        /// Trigger the connect button of the phone
        /// </summary>
        public void Connect()
        {
            ConnectControl.Trigger();
        }

        /// <summary>
        /// Returns true if call is connected
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool Connected { get; protected set; }

        private DateTime _CallConnectedTime;

        /// <summary>
        /// Time of current call duration
        /// </summary>
        public TimeSpan CallTimer
        {
            get
            {
                if (_CallConnectedTime != null)
                    return DateTime.Now - _CallConnectedTime;
                else return TimeSpan.FromSeconds(0);
            }
        }

        /// <summary>
        /// Returns true if incoming call
        /// </summary>
        [System.ComponentModel.DefaultValue(false)]
        public bool IncomingCall { get; protected set; }

        /// <summary>
        /// Trigger the disconnect button of the phone
        /// </summary>
        public void Disconnect()
        {
            DisconnectControl.Trigger();
        }

        /// <summary>
        /// Set or Get the current dial string of the dialer
        /// </summary>
        public string DialString
        {
            get
            {
                return DialStringControl.StringValue;
            }
            set
            {
                DialStringControl.StringValue = value;
            }
        }

        /// <summary>
        /// The last dialled number
        /// </summary>
        public string LastDialledNumber
        {
            get;
            protected set;
        }

        /// <summary>
        /// State of the Ringer LED
        /// </summary>
        public bool IsRinging
        {
            get
            {
                if (RingingLEDControl.ControlPosition == 1)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Get the current caller display name of the connected call
        /// </summary>
        [System.ComponentModel.DefaultValue("")]
        public string CallDisplayName { get; protected set; }

        /// <summary>
        /// Set or get the Do Not Disturb state
        /// </summary>
        public bool DND
        {
            get
            {
                if (DNDControl.ControlPosition == 1)
                    return true;
                return false;
            }
            set
            {
                if (value) DNDControl.ControlPosition = 1;
                else DNDControl.ControlPosition = 0;
            }
        }

        /// <summary>
        /// Trigger the keypad digit
        /// </summary>
        /// <param name="digit">String value of digit 0-9,*,#</param>
        public void TriggerKeypad(string digit)
        {
            KeypadControls[digit].Trigger();
        }

        /// <summary>
        /// Something has changed on one of the properties of the phone.
        /// </summary>
        public event QSysSoftPhoneStatusChangeEventHandler StatusChanged;

        QSysPhoneCallState _CallStatus = QSysPhoneCallState.Idle;
        public QSysPhoneCallState CallStatus
        {
            get
            {
                return _CallStatus;
            }
            protected set
            {
                if (_CallStatus != value)
                {
                    _CallStatus = value;
                    if (value == QSysPhoneCallState.Dialing)
                        this.LastDialledNumber = this.CallDisplayName;
                    if (value == QSysPhoneCallState.Connected)
                        this._CallConnectedTime = DateTime.Now;
                    if (CallStatusChanged != null)
                        CallStatusChanged(this, new QSysSoftPhoneCallStatusChangeEventArgs(this, value));
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public event QSysSoftPhoneCallStatusChangeEventHandler CallStatusChanged;

        void OnValueChange(QSysControl control)
        {
            if (StatusChanged != null)
            {
                if (control == RingingLEDControl)
                {
                    StatusChanged(this, new QSysSoftPhoneStatusChangeEventArgs(QSysSoftPhoneStatusChangeEventID.IsRingingStatusChanged));
                    if (this.IsRinging)
                        CallStatus = QSysPhoneCallState.Ringing;
                }
                if (control == DialStringControl)
                    StatusChanged(this, new QSysSoftPhoneStatusChangeEventArgs(QSysSoftPhoneStatusChangeEventID.DialStringChanged));
                if (control == DNDControl)
                    StatusChanged(this, new QSysSoftPhoneStatusChangeEventArgs(QSysSoftPhoneStatusChangeEventID.DNDStatusChanged));
                if (control == OffHookLEDControl)
                    StatusChanged(this, new QSysSoftPhoneStatusChangeEventArgs(QSysSoftPhoneStatusChangeEventID.OffHookStatusChanged));
            }

            if (control == ProgressControl)
            {
                if (StatusChanged != null)
                    StatusChanged(this, new QSysSoftPhoneStatusChangeEventArgs(QSysSoftPhoneStatusChangeEventID.ProgressStatusChanged));

                if (control.StringValue == "Disconnected")
                {
                    this.Connected = false;
                    this.CallDisplayName = string.Empty;
                    this.IncomingCall = false;
                    CallStatus = QSysPhoneCallState.Disconnected;
                }

                if (control.StringValue.StartsWith("Dialing"))
                {
                    this.CallDisplayName = control.StringValue.Split(' ').Last();
                    CallStatus = QSysPhoneCallState.Dialing;
                }

                else if (control.StringValue.StartsWith("Call in progress") || control.StringValue.StartsWith("Connected"))
                {
                    this.CallDisplayName = control.StringValue.Split(' ').Last();
                    this.Connected = true;
                    this.IncomingCall = false;
                    CallStatus = QSysPhoneCallState.Connected;
                }

                else if (control.StringValue.StartsWith("Incoming call from: "))
                {
                    this.CallDisplayName = control.StringValue.Remove(0, 20);
                    this.IncomingCall = true;
                    CallStatus = QSysPhoneCallState.Incoming;
                }

                else if (control.StringValue == "Idle")
                {
                    CallStatus = QSysPhoneCallState.Idle;
                }
            }
        }
    }

    /// <summary>
    /// Handles the status change on the properties of the phone.
    /// </summary>
    /// <param name="phone">The instance of the phone</param>
    /// <param name="args">QSysSoftPhoneStatusChangeEventArgs information</param>
    public delegate void QSysSoftPhoneStatusChangeEventHandler(QSysSoftPhone phone, QSysSoftPhoneStatusChangeEventArgs args);

    /// <summary>
    /// QSysSoftPhoneStatusChangeEventHandler args
    /// </summary>
    public class QSysSoftPhoneStatusChangeEventArgs
    {
        internal QSysSoftPhoneStatusChangeEventArgs(int eventID)
        {
            this.EventID = eventID;
        }

        /// <summary>
        /// The QSysSoftPhoneStatusChangeEventID value
        /// </summary>
        public int EventID { get; internal set; }
    }

    /// <summary>
    /// Values for the QSysSoftPhoneStatusChangeEventHandler
    /// </summary>
    public static class QSysSoftPhoneStatusChangeEventID
    {
        public const int OffHookStatusChanged = 1;
        public const int IsRingingStatusChanged = 2;
        public const int DialStringChanged = 3;
        public const int DNDStatusChanged = 4;
        public const int ProgressStatusChanged = 5;
    }

    /// <summary>
    /// State of the call
    /// </summary>
    public enum QSysPhoneCallState
    {
        Idle,
        Dialing,
        Incoming,
        Ringing,
        Connected,
        Disconnected
    }

    /// <summary>
    /// Handles a call status change
    /// </summary>
    /// <param name="phone">The instance of the phone</param>
    /// <param name="args">QSysSoftPhoneCallStatusChangeEventArgs information</param>
    public delegate void QSysSoftPhoneCallStatusChangeEventHandler(QSysSoftPhone phone, QSysSoftPhoneCallStatusChangeEventArgs args);

    /// <summary>
    /// Args for call status change on the phone
    /// </summary>
    public class QSysSoftPhoneCallStatusChangeEventArgs
    {
        internal QSysSoftPhoneCallStatusChangeEventArgs(QSysSoftPhone phone, QSysPhoneCallState state)
        {
            Phone = phone;
            State = state;
        }

        QSysSoftPhone Phone { get; set; }

        /// <summary>
        /// Current State of the call
        /// </summary>
        public QSysPhoneCallState State { get; protected set; }

        /// <summary>
        /// True if the phone is off hook
        /// </summary>
        public bool InCall
        {
            get { return Phone.OffHook; }
        }

        /// <summary>
        /// True if the phone is ringing in or out
        /// </summary>
        public bool Ringing
        {
            get { return Phone.IsRinging; }
        }

        /// <summary>
        /// If the call is actually connected to 3rd party this will be true
        /// </summary>
        public bool CallConnected
        {
            get { return Phone.Connected; }
        }

        /// <summary>
        /// The caller display name or number of the call
        /// </summary>
        public string CallDisplayName
        {
            get { return Phone.CallDisplayName; }
        }

        /// <summary>
        /// True if incoming call
        /// </summary>
        public bool IncomingCall
        {
            get { return Phone.IncomingCall; }
        }
    }
}