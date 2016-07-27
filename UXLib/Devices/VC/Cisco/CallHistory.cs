using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class CallHistory : IEnumerable<CallHistoryItem>
    {
        public CallHistory(CiscoCodec codec, int limit)
        {
            this.Codec = codec;

            CommandArgs args = new CommandArgs();

            args.Add("Filter", "All");
            args.Add("DetailLevel", "Full");
            args.Add("Limit", limit);

            bool useHttp = false;
            if (limit > 0) useHttp = true;

            XDocument xml = Codec.SendCommand("Command/CallHistory/Get", args, useHttp);
#if DEBUG
            CrestronConsole.PrintLine("Callhistory: \r\n{0}", xml.ToString());
#endif
            foreach (XElement item in xml.Root.Elements().Elements("Entry"))
            {

                CallHistoryItem call = new CallHistoryItem(Codec,
                    int.Parse(item.Element("CallHistoryId").Value),
                    int.Parse(item.Element("CallId").Value));

                calls.Add(call.ID, call);

                if (item.Element("CallbackNumber") != null)
                    call.CallbackNumber = item.Element("CallbackNumber").Value;
                if (item.Element("RemoteNumber") != null)
                    call.RemoteNumber = item.Element("RemoteNumber").Value;
                if (item.Element("DisplayName") != null)
                    call.DisplayName = item.Element("DisplayName").Value;
                if (item.Element("Direction") != null)
                    call.Direction = (CallDirection)Enum.Parse(
                        typeof(CallDirection), item.Element("Direction").Value, false);
                if (item.Element("CallType") != null)
                    call.Type = (CallType)Enum.Parse(
                        typeof(CallType), item.Element("CallType").Value, false);
                if (item.Element("OccurrenceType") != null)
                    call.OccurrenceType = (CallOccurrenceType)Enum.Parse(
                        typeof(CallOccurrenceType), item.Element("OccurrenceType").Value, false);
                if (item.Element("Protocol") != null)
                    call.Protocol = item.Element("Protocol").Value;
                if (item.Element("StartTime") != null)
                    call.StartTime = DateTime.Parse(item.Element("StartTime").Value);
                if (item.Element("EndTime") != null)
                    call.EndTime = DateTime.Parse(item.Element("EndTime").Value);
                if (item.Element("DisconnectCause") != null)
                    call.DisconnectCause = item.Element("DisconnectCause").Value;
                if (item.Element("DisconnectCauseType") != null)
                    call.DisconnectCauseType = (CallDisconnectCauseType)Enum.Parse(
                    typeof(CallDisconnectCauseType), item.Element("DisconnectCauseType").Value, false);
            }
        }

        CiscoCodec Codec;

        Dictionary<int, CallHistoryItem> calls = new Dictionary<int, CallHistoryItem>();

        public CallHistoryItem this[int callHistoryID]
        {
            get
            {
                return calls[callHistoryID];
            }
        }

        public int Count { get { return calls.Count; } }

        #region IEnumerable<CallHistoryItem> Members

        public IEnumerator<CallHistoryItem> GetEnumerator()
        {
            return calls.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        #endregion
    }
}