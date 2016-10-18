using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices
{
    public interface ICommDevice
    {
        bool DeviceCommunicating { get; }
        void Send(string stringToSend);
        void OnReceive(string receivedString);
        void Initialize();
        CommDeviceType CommunicationType { get; }
        event ICommDeviceDeviceCommunicatingChangeEventHandler DeviceCommunicatingChanged;
    }

    public enum CommDeviceType
    {
        Serial,
        IP,
        OneWayIRSerial
    }

    public delegate void ICommDeviceDeviceCommunicatingChangeEventHandler(ICommDevice device, bool communicating);
}