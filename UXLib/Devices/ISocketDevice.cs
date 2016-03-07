using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices
{
    public interface ISocketDevice : ICommDevice 
    {
        string HostAddress { get; }
        void Connect();
        void Disconnect();
        bool Connected { get; }
    }
}