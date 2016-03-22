using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

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
                var ts = new TimeSpan(DateTime.UtcNow.Ticks - this.StartTime.Ticks);
                double delta = Math.Abs(ts.TotalSeconds);

                if (delta < 60)
                {
                    return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds ago";
                }
                if (delta < 120)
                {
                    return "a minute ago";
                }
                if (delta < 2700) // 45 * 60
                {
                    return ts.Minutes + " minutes ago";
                }
                if (delta < 5400) // 90 * 60
                {
                    return "an hour ago";
                }
                if (delta < 86400) // 24 * 60 * 60
                {
                    return ts.Hours + " hours ago";
                }
                if (delta < 172800) // 48 * 60 * 60
                {
                    return "yesterday";
                }
                if (delta < 2592000) // 30 * 24 * 60 * 60
                {
                    return ts.Days + " days ago";
                }
                if (delta < 31104000) // 12 * 30 * 24 * 60 * 60
                {
                    int months = Convert.ToInt32(Math.Floor((double)ts.Days / 30));
                    return months <= 1 ? "one month ago" : months + " months ago";
                }
                int years = Convert.ToInt32(Math.Floor((double)ts.Days / 365));
                return years <= 1 ? "one year ago" : years + " years ago";
            }
        }
        public CallDirection Direction { get; set; }
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

        public int Callback()
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