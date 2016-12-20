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
            this.Ghost = false;
        }

        public CiscoCodec Codec;

        public int ID { get; protected set; }
        public bool Ghost { get; internal set; }
        public CallStatus Status { get; internal set; }
        public CallDirection Direction { get; internal set; }
        public CallType Type { get; internal set; }
        public string RemoteNumber { get; internal set; }
        public string CallbackNumber { get; internal set; }
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
            internal set
            {
                _DisplayName = value;
            }
        }
        public CallDeviceType DeviceType { get; internal set; }
        public string Protocol { get; internal set; }
        public CallAnswerState AnswerState { get; internal set; }
        public DateTime StartTime { get; internal set; }
        public TimeSpan Duration { get { return DateTime.Now - this.StartTime; } }

        public bool InProgress
        {
            get
            {
                switch (this.Status)
                {
                    case CallStatus.Dialling:
                    case CallStatus.Ringing:
                    case CallStatus.Disconnecting:
                    case CallStatus.Connecting:
                        return true;
                    default:
                        return false;
                }
            }
        }

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
            SendCommand("Resume");
        }

        public void Join()
        {
            SendCommand("Join");
        }

        public void DTMFSend(string dtmfString)
        {
            CommandArgs args = new CommandArgs("CallId", this.ID);
            args.Add(new CommandArg("DTMFString", dtmfString));
            Codec.SendCommand("Call/DTMFSend", args);
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