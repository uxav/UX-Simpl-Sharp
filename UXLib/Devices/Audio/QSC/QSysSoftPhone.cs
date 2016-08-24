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
        public QSysSoftPhone(QSys device, string idOffHookLED, string idRingingLED, string idConnect, string idDisconnect,
            string idDialString, string idDND, string idProgress, string idKeypadBaseName)
        {
            this.QSys = device;
            KeypadControls = new Dictionary<string, QSysControl>();
            OffHookLEDControl = this.QSys.Controls.Register(idOffHookLED, QSysControlType.Other, 2);
            ConnectControl = this.QSys.Controls.Register(idConnect, QSysControlType.Other);
            DisconnectControl = this.QSys.Controls.Register(idDisconnect, QSysControlType.Other);
            DialStringControl = this.QSys.Controls.Register(idDialString, QSysControlType.Other, 2);
            RingingLEDControl = this.QSys.Controls.Register(idRingingLED, QSysControlType.Other, 2);
            DNDControl = this.QSys.Controls.Register(idDND, QSysControlType.Other, 2);
            ProgressControl = this.QSys.Controls.Register(idProgress, QSysControlType.Other, 2);

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
                if (OffHookLEDControl.ControlPosition == 1)
                    return true;
                return false;
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

        public event QSysSoftPhoneStatusChangeEventHandler StatusChanged;

        void OnValueChange(QSysControl control)
        {
            if(StatusChanged != null) {
                if (control == OffHookLEDControl)
                    StatusChanged(this, new QSysSoftPhoneStatusChangeEventArgs(QSysSoftPhoneStatusChangeEventID.OffHookStatusChanged));
                if (control == RingingLEDControl)
                    StatusChanged(this, new QSysSoftPhoneStatusChangeEventArgs(QSysSoftPhoneStatusChangeEventID.IsRingingStatusChanged));
            }
        }
    }

    public delegate void QSysSoftPhoneStatusChangeEventHandler(QSysSoftPhone phone, QSysSoftPhoneStatusChangeEventArgs args);

    public class QSysSoftPhoneStatusChangeEventArgs
    {
        internal QSysSoftPhoneStatusChangeEventArgs(int eventID)
        {
            this.EventID = eventID;
        }

        public int EventID { get; internal set; }
    }

    public static class QSysSoftPhoneStatusChangeEventID
    {
        public const int OffHookStatusChanged = 1;
        public const int IsRingingStatusChanged = 2;
        public const int DialStringChanged = 3;
        public const int DNDStatusChanged = 4;
        public const int ProgressStatusChanged = 5;
    }
}