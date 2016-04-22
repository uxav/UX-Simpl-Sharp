using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.Polycom
{
    public interface ISoundstructurePhoneOutChannel
    {
        bool OffHook { get; set; }
        void Dial(string number);
        void Reject();
        void Answer();
        void Ignore();
    }
}