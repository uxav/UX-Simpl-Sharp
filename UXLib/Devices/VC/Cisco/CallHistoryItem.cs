using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Extensions;

namespace UXLib.Devices.VC.Cisco
{
    public class CallHistoryItem
    {
        public CallHistoryItem(CiscoCodec codec, int callHistoryID, int callID)
        {
            this.Codec = codec;
            this.ID = callHistoryID;
            this.CallID = callID;
        }

        public CiscoCodec Codec { get; protected set; }
        public int ID { get; protected set; }
        public int CallID { get; protected set; }
        public CallOccurrenceType OccurrenceType { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get { return this.EndTime - this.StartTime; } }
        public bool OccuredToday
        {
            get
            {
                if (this.StartTime.Date == DateTime.Today) return true;
                return false;
            }
        }
        public string RelativeTime
        {
            get
            {
                return new TimeSpan(DateTime.Now.Ticks - this.StartTime.Ticks).ToPrettyTimeAgo();
            }
        }
        public CallDirection Direction { get; set; }
        public CallType Type { get; set; }
        public string Protocol { get; set; }
        public string RemoteNumber { get; set; }
        public string CallbackNumber { get; set; }
        public string DisplayName { get; set; }
        public string DisconnectCause { get; set; }
        public CallDisconnectCauseType DisconnectCauseType { get; set; }

        public void AcknowledgeMissedCall()
        {
            Codec.SendCommand("Call/AcknowledgeMissedCall", new CommandArgs("CallHistoryId", this.ID));
        }

        public DialResult Callback()
        {
            return Codec.Calls.Dial(this.CallbackNumber);
        }
    }

    public enum CallDisconnectCauseType
    {
        OtherLocal,
        LocalDisconnect,
        UnknownRemoteSite,
        LocalBusy,
        LocalReject,
        InsufficientSecurity,
        OtherRemote,
        RemoteDisconnect,
        RemoteBusy,
        RemoteRejected,
        RemoteNoAnswer,
        CallForwarded,
        NetworkRejected
    }

    public enum CallOccurrenceType
    {
        Missed,
        AnsweredElsewhere,
        Forwarded,
        Placed,
        NoAnswer,
        Received,
        Rejected,
        UnacknowledgedMissed
    }
}