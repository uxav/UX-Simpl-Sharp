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
using UXLib.Models;
using UXLib.Sockets;

namespace UXLib.Devices.Displays.VividTouch
{
    public class VividTouchDisplay : DisplayDevice, IVolumeDevice
    {
        private readonly VividTouchComPortHandler _comPortHandler;

        public VividTouchDisplay(ComPort comPort, string name)
        {
            Name = name;
            _comPortHandler = new VividTouchComPortHandler(comPort);
            _comPortHandler.ReceivedData += ComPortHandlerOnReceivedData;
        }

        private CTimer _pollTimer;

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
                CrestronConsole.PrintLine("Power = {0}", Convert.ToBoolean(bytes[6]));
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
                CrestronConsole.PrintLine("Actual input = {0}, requested input = {1}", bytes[6], requestedInput);
#endif

                if (requestedInput != 0xff && requestedInput != bytes[6])
                    Send(VividTouchMessageType.Write, new byte[] {0x4d, 0x49, 0x4e, requestedInput});
            }

            else if (bytes[3] == 0x56 && bytes[4] == 0x4f && bytes[5] == 0x4c)
            {
                var v = Convert.ToUInt32(bytes[6]);
                if (!_volume.Equals(v))
                {
                    _volume = v;
                    OnVolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.LevelChanged));
                }
            }
        }

        private void ComPortHandlerOnReceivedData(byte[] receivedData)
        {
            OnReceive(receivedData);
        }

        private void PollPower()
        {
            Send(VividTouchMessageType.Read, new byte[] { 0x50, 0x4f, 0x57 });
        }

        private void PollPCPower()
        {
            Send(VividTouchMessageType.Read, new byte[] { 0x49, 0x50, 0x43 });
        }

        private void PollInput()
        {
            Send(VividTouchMessageType.Read, new byte[] {0x4d, 0x49, 0x4e});
        }

        private void PollVolume()
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
                case DisplayDeviceInput.BuiltIn:
                    return 0x0e;
                case DisplayDeviceInput.VGA:
                    return 0x00;
                default:
                    return 0xff;
            }
        }

        private uint _volume;

        public uint Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                Send(VividTouchMessageType.Write, new byte[] {0x56, 0x4f, 0x4c, (byte) _volume});
            }
        }

        public override void Initialize()
        {
            _pollTimer = new CTimer(PollDisplayStep, null, 1000, 1000);
            CrestronEnvironment.ProgramStatusEventHandler += type =>
            {
                if (type != eProgramStatusEventType.Stopping) return;
                _pollTimer.Stop();
                _pollTimer.Dispose();
            };
        }

        private int _pollCount;

        private void PollDisplayStep(object userSpecific)
        {
            _pollCount ++;

            switch (_pollCount)
            {
                case 1:
                    PollPower();
                    break;
                case 2:
                    PollPCPower();
                    break;
                case 3:
                    if (PowerStatus == DevicePowerStatus.PowerOn)
                    {
                        PollInput();
                        PollVolume();
                    }
                    else
                    {
                        _pollCount = 0;
                    }
                    break;
            }

            if (_pollCount > 9)
                _pollCount = 0;
        }

        public override CommDeviceType CommunicationType
        {
            get { return CommDeviceType.Serial; }
        }

        #region Implementation of IVolumeDevice

        /// <summary>
        /// Set or Get the volume level
        /// </summary>
        public ushort VolumeLevel
        {
            get { return (ushort) Tools.ScaleRange(_volume, 0, 100, ushort.MinValue, ushort.MaxValue); }
            set { Volume = (uint) Tools.ScaleRange(value, ushort.MinValue, ushort.MaxValue, 0, 100); }
        }

        /// <summary>
        /// Set or Get the mute
        /// </summary>
        public bool VolumeMute { get; set; }

        /// <summary>
        /// Return true if this supports Mute control
        /// </summary>
        public bool SupportsVolumeMute
        {
            get { return true; }
        }

        /// <summary>
        /// Returns true id this supports Level control
        /// </summary>
        public bool SupportsVolumeLevel
        {
            get { return true; }
        }

        public event VolumeDeviceChangeEventHandler VolumeChanged;

        protected virtual void OnVolumeChanged(IVolumeDevice device, VolumeChangeEventArgs args)
        {
            var handler = VolumeChanged;
            if (handler != null) handler(device, args);
        }

        #endregion
    }
}
