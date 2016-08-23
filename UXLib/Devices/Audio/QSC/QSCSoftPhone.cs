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
    public class QSCSoftPhone
    {
        public QSCSoftPhone(QSys device, string idOffHookLED, string idRingingLED, string idConnect, string idDisconnect,
            string idDialString, string idDND)
        {
            this.QSys = device;

            OffHookLEDControl = this.QSys.Controls.Register(idOffHookLED, QSysControlType.Other, 2);
            ConnectControl = this.QSys.Controls.Register(idConnect, QSysControlType.Other, 2);
            DisconnectControl = this.QSys.Controls.Register(idDisconnect, QSysControlType.Other, 2);
            DialStringControl = this.QSys.Controls.Register(idDialString, QSysControlType.Other, 2);
            RingingLEDControl = this.QSys.Controls.Register(idRingingLED, QSysControlType.Other, 2);
            DNDControl = this.QSys.Controls.Register(idDND, QSysControlType.Other);
        }

        public QSys QSys { get; protected set; }

        QSysControl OffHookLEDControl { set; get; }
        QSysControl ConnectControl { set; get; }
        QSysControl DisconnectControl { set; get; }
        QSysControl DialStringControl { set; get; }
        QSysControl RingingLEDControl { set; get; }
        QSysControl DNDControl { set; get; }

        public bool OffHook
        {
            get
            {
                if (OffHookLEDControl.ControlPosition == 1)
                    return true;
                return false;
            }
        }

        public void Connect()
        {
            ConnectControl.Trigger();
        }

        public void Disconnect()
        {
            DisconnectControl.Trigger();
        }

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

        public bool IsRinging
        {
            get
            {
                if (RingingLEDControl.ControlPosition == 1)
                    return true;
                return false;
            }
        }

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
    }
}