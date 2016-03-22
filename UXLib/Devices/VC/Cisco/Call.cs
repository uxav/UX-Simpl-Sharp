using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class Call
    {
        public Call(CiscoCodec codec, int id)
        {
            this.Codec = codec;
            this.ID = id;
            this.StartTime = DateTime.Now;
        }

        public CiscoCodec Codec;

        public int ID { get; protected set; }
        public CallStatus Status { get; set; }
        public CallDirection Direction { get; set; }
        public CallType Type { get; set; }
        public string RemoteNumber { get; set; }
        public string CallbackNumber { get; set; }
        string _DisplayName = string.Empty;
        public string DisplayName
        {
            get
            {
                if (_DisplayName.Length > 0)
                    return _DisplayName;
                else
                    return RemoteNumber;
            }
            set
            {
                _DisplayName = value;
            }
        }
        public CallDeviceType DeviceType { get; set; }
        public string Protocol { get; set; }
        public CallAnswerState AnswerState { get; set; }
        public DateTime StartTime { get; set; }
        public TimeSpan Duration { get { return DateTime.Now - this.StartTime; } }

        public void Disconnect()
        {
            SendCommand("Disconnect");
        }

        public void Reject()
        {
            SendCommand("Reject");
        }

        public void Accept()
        {
            SendCommand("Accept");
        }

        public void Ignore()
        {
            SendCommand("Ignore");
        }

        public bool OnHold
        {
            get
            {
                if (Status == CallStatus.OnHold)
                    return true;
                else
                    return false;
            }
            set
            {
                if (Status == CallStatus.Connected && value)
                    this.Hold();
                else if (this.OnHold && !value)
                    this.Resume();
            }
        }

        public void Hold()
        {
            SendCommand("Hold");
        }

        public void Resume()
        {
            SendCommand("Hold");
        }

        public void Join()
        {
            SendCommand("Join");
        }

        public void DTMFSend(string dtmfString)
        {
            SendCommand("DTMFSend", new CommandArg("DTMFString", dtmfString));
        }

        void SendCommand(string command)
        {
            Codec.SendCommand(string.Format("Call/{0}", command), new CommandArgs("CallId", this.ID));
        }

        void SendCommand(string command, CommandArg arg)
        {
            CommandArgs args = new CommandArgs("CallId", this.ID);
            args.Add(arg);
            Codec.SendCommand(string.Format("Call/{0}", command), args);
        }
    }

    public enum CallStatus
    {
        Idle,
        Dialling,
        Ringing,
        Connecting,
        Connected,
        Disconnecting,
        OnHold,
        EarlyMedia,
        Preserved,
        RemotePreserved
    }

    public enum CallDirection
    {
        Incoming,
        Outgoing
    }

    public enum CallType
    {
        Video,
        Audio,
        AudioCanEscalate,
        ForwardAllCall,
        Unknown
    }

    public enum CallDeviceType
    {
        Endpoint,
        MCU
    }

    public enum CallAnswerState
    {
        Unanswered,
        Ignored,
        Autoanswered,
        Answered
    }
}