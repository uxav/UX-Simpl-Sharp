/* License
 * ------------------------------------------------------------------------------
 * Copyright (c) 2017 UX Digital Systems Ltd
 * 
 * Permission is hereby granted, to any person obtaining a copy of this software
 * and associated documentation files (the "Software"), to deal in the Software
 * for the continued use and development of the system on which it was installed,
 * and to permit persons to whom the Software is furnished to do so, subject to
 * the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * Any persons obtaining the software have no rights to use, copy, modify, merge,
 * publish, distribute, sublicense, and/or sell copies of the Software without
 * written persmission from UX Digital Systems Ltd, if it is not for use on the
 * system on which it was originally installed.
 * ------------------------------------------------------------------------------
 * UX.Digital
 * ----------
 * http://ux.digital
 * support@ux.digital
 */

using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using UXLib.Sockets;

namespace UXLib.Devices.Displays.VividTouch
{
    public class VividTouchDisplay : DisplayDevice
    {
        private VividTouchComPortHandler _comPortHandler;

        public VividTouchDisplay(ComPort comPort, string name)
        {
            this.Name = name;
            _comPortHandler = new VividTouchComPortHandler(comPort);
            _comPortHandler.ReceivedData += ComPortHandlerOnReceivedData;
        }

        private CTimer pollTimer { get; set; }

        private void Socket_SocketConnectionEvent(SimpleClientSocket socket,
            Crestron.SimplSharp.CrestronSockets.SocketStatus status)
        {
            if (status == Crestron.SimplSharp.CrestronSockets.SocketStatus.SOCKET_STATUS_CONNECTED)
            {
                pollTimer = new CTimer(PollPower, null, 1000, 10000);
            }
            else if (pollTimer != null)
            {
                pollTimer.Dispose();
            }
        }

        public void Send(VividTouchMessageType messageType, byte[] bytes)
        {
            _comPortHandler.Send(0x01, messageType, bytes);
        }

        public override void OnReceive(byte[] bytes)
        {
            base.OnReceive(bytes);

            if (bytes[3] == 0x50 && bytes[4] == 0x4f && bytes[5] == 0x57)
            {
#if DEBUG
                //CrestronConsole.PrintLine("Power = {0}", Convert.ToBoolean(bytes[6]));
#endif
                switch (Convert.ToBoolean(bytes[6]))
                {
                    case true:
                        PowerStatus = DevicePowerStatus.PowerOn;
                        if (!RequestedPower)
                        {
                            // Send power as should be off
                            SendPowerCommand(false);
                        }
                        else
                        {
                            RequestedPower = true;
                        }
                        break;
                    case false:
                        PowerStatus = DevicePowerStatus.PowerOff;
                        if (RequestedPower)
                            SendPowerCommand(true);
                        break;
                }
            }

            else if (bytes[3] == 0x4d && bytes[4] == 0x49 && bytes[5] == 0x4e)
            {
#if DEBUG
                //CrestronConsole.PrintLine("Actual input = {0}, requested input = {1}", bytes[6], requestedInput);
#endif

                if (requestedInput != 0xff && requestedInput != bytes[6])
                    Send(VividTouchMessageType.Write, new byte[] {0x4d, 0x49, 0x4e, requestedInput});
            }

            else if (bytes[3] == 0x56 && bytes[4] == 0x4f && bytes[5] == 0x4c)
            {
                if (_volume != Convert.ToUInt32(bytes[6]) && !volumeOK)
                    Volume = _volume;
                else if (_volume == Convert.ToUInt32(bytes[6]) && !volumeOK)
                    volumeOK = true;
            }
        }

        private void ComPortHandlerOnReceivedData(byte[] receivedData)
        {
            OnReceive(receivedData);
        }

        private void PollPower(object callBackObject)
        {
            Send(VividTouchMessageType.Read, new byte[] {0x50, 0x4f, 0x57});
            if (PowerStatus == DevicePowerStatus.PowerOn)
                new CTimer(PollInput, null, 100);
        }

        private void PollInput(object callBackObject)
        {
            Send(VividTouchMessageType.Read, new byte[] {0x4d, 0x49, 0x4e});
            if (PowerStatus == DevicePowerStatus.PowerOn)
                new CTimer(PollVolume, null, 100);
        }

        private void PollVolume(object callBackObject)
        {
            Send(VividTouchMessageType.Read, new byte[] {0x56, 0x4f, 0x4c});
        }

        private void SendPowerCommand(bool power)
        {
            if (power)
            {
                Send(VividTouchMessageType.Write, new byte[] {0x50, 0x4f, 0x57, 0x01});
            }
            else if (!power)
            {
                Send(VividTouchMessageType.Write, new byte[] {0x50, 0x4f, 0x57, 0x00});
            }
        }

        public override bool Power
        {
            get { return base.Power; }
            set
            {
                base.Power = value;
                SendPowerCommand(value);
            }
        }

        private byte requestedInput = 0xff;

        public override DisplayDeviceInput Input
        {
            get { return base.Input; }
            set
            {
                requestedInput = InputCommandForInput(value);
                if (requestedInput == 0xff) return;
                Send(VividTouchMessageType.Write, new byte[] {0x4d, 0x49, 0x4e, requestedInput});
                base.Input = value;
            }
        }

        public byte InputCommandForInput(DisplayDeviceInput input)
        {
            switch (input)
            {
                case DisplayDeviceInput.HDMI1:
                    return 0x09;
                case DisplayDeviceInput.HDMI2:
                    return 0x0a;
                case DisplayDeviceInput.HDMI3:
                    return 0x0b;
                case DisplayDeviceInput.HDMI4:
                    return 0x0c;
                case DisplayDeviceInput.DisplayPort:
                    return 0x0d;
                case DisplayDeviceInput.VGA:
                    return 0x00;
                default:
                    return 0xff;
            }
        }

        private uint _volume;
        private bool volumeOK = true;

        public uint Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                volumeOK = false;
                Send(VividTouchMessageType.Write, new byte[] {0x56, 0x4f, 0x4c, (byte) _volume});
            }
        }

        #region ICommDevice Members

        public override void Initialize()
        {
            
        }

        public override CommDeviceType CommunicationType
        {
            get { return CommDeviceType.Serial; }
        }

        #endregion
    }
}
