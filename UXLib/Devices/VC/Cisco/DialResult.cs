using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class DialResult
    {
        public DialResult(int callId)
        {
            CallId = callId;
        }

        public DialResult(int errorCause, string description)
        {
            Error = true;
            ErrorCause = errorCause;
            ErrorDescription = description;
        }

        public bool Error;
        public int ErrorCause;
        public string ErrorDescription;
        public int CallId;
    }
}