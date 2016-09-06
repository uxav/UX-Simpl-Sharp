using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace UXLib.Extensions
{
    public static class ComPortExtensions
    {
        public static void Send(this ComPort port, byte[] bytes, int count)
        {
            string str = string.Empty;
            for (int i = 0; i < count; i++)
            {
                str = str + (char)bytes[i];
            }
            port.Send(str);
        }

        public static void SendSerialData(this IROutputPort port, byte[] bytes, int count)
        {
            string str = string.Empty;
            for (int i = 0; i < count; i++)
            {
                str = str + (char)bytes[i];
            }
            port.SendSerialData(str);
        }
    }
}