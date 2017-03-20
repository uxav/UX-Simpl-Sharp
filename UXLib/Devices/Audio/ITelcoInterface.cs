using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio
{
    public interface ITelcoInterface
    {
        bool OffHook { get; set; }
        void Dial(string number);
        void Reject();
        void Answer();
        void Ignore();
        event TelcoOffHookStatusChangeEventHandler OffHookChange;
    }

    public delegate void TelcoOffHookStatusChangeEventHandler(ITelcoInterface telco, bool hookStatus);
}