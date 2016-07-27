using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class Calls : IEnumerable<Call>
    {
        public Calls(CiscoCodec codec)
        {
            this.Codec = codec;
            this.Codec.FeedbackServer.ReceivedData += new CodecFeedbackServerReceiveEventHandler(FeedbackServer_ReceivedData);
            this.Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
        }

        CiscoCodec Codec;

        Dictionary<int, Call> calls = new Dictionary<int, Call>();
        Dictionary<int, Call> persistantCalls = new Dictionary<int, Call>();

        public Call this[int callID]
        {
            get
            {
                return calls[callID];
            }
        }

        public ReadOnlyDictionary<int, Call> CallLog { get { return new ReadOnlyDictionary<int, Call>(persistantCalls); } }

        public int Count { get { return calls.Count; } }

        public int Dial(CommandArgs args)
        {
            try
            {
                if (Codec.SystemUnit.State.NumberOfActiveCalls < Codec.Capabilities.Conference.MaxActiveCalls)
                {
                    XDocument xml = Codec.SendCommand("Dial", args, true);
                    XElement dialResult = xml.Root.Element("DialResult");
                    if (dialResult.Attribute("status").Value == "OK")
                    {
                        int callID = int.Parse(dialResult.Element("CallId").Value);
                        if (!calls.ContainsKey(callID))
                        {
                            calls.Add(callID, new Call(Codec, callID));
                            if (args.ContainsArg("CallType"))
                                calls[callID].Type = (CallType)Enum.Parse(typeof(CallType), args["CallType"].Value, false);
                            calls[callID].RemoteNumber = args["Number"].Value;
                            calls[callID].Direction = CallDirection.Outgoing;
                            calls[callID].Status = CallStatus.Dialling;
                            OnCallStatusChange(calls[callID]);
                        }
                        return callID;
                    }
                }
                else
                {
                    ErrorLog.Warn("Codec: Could not dial, NumberOfActiveCalls = {0}, MaxActiveCalls = {1}",
                        Codec.SystemUnit.State.NumberOfActiveCalls, Codec.Capabilities.Conference.MaxActiveCalls);
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in Codec.Dial(), {0}", e.Message);
            }

            return 0;
        }

        public int Dial(string number)
        {
            CommandArgs args = new CommandArgs("Number", number);
            return Dial(args);
        }

        public int Dial(string number, CallType callType)
        {
            CommandArgs args = new CommandArgs("Number", number);
            args.Add("CallType", callType.ToString());
            return Dial(args);
        }

        public void Disconnect()
        {
            Codec.SendCommand("Call/Disconnect", new CommandArgs());
        }

        public void Disconnect(int callID)
        {
            if (calls.ContainsKey(callID))
                Codec.SendCommand("Call/Disconnect", new CommandArgs("CallId", callID));
        }

        public event CodecCallInfoChangeEventHandler CallStatusChange;

        void OnCallStatusChange(Call call)
        {
            if (CallStatusChange != null)
                CallStatusChange(Codec, new CodecCallInfoChangeEventArgs(call));
        }

        void OnCallStatusChange(Call call, bool hasDisconnected)
        {
            if (CallStatusChange != null)
            {
                if (hasDisconnected)
                {
                    CallStatusChange(Codec, new CodecCallInfoChangeEventArgs(call, true));
                }
                else
                {
                    OnCallStatusChange(call);
                }
            }
        }

        void FeedbackServer_ReceivedData(CodecFeedbackServer server, CodecFeedbackServerReceiveEventArgs args)
        {
            try
            {
                switch (args.Path)
                {
                    case @"Status/Call":
                        int callID = int.Parse(args.Data.Attribute("item").Value);
                        bool ghost = args.Data.Attribute("ghost") != null ? bool.Parse(args.Data.Attribute("ghost").Value) : false;

                        if (ghost && calls.ContainsKey(callID))
                        {
                            if (calls[callID].Status != CallStatus.Idle)
                                calls[callID].Status = CallStatus.Idle;
                            Call disconnectedCall = calls[callID];
                            calls.Remove(callID);
                            OnCallStatusChange(disconnectedCall, true);
                        }
                        else if (!ghost)
                        {
                            if (!calls.ContainsKey(callID))
                                calls.Add(callID, new Call(Codec, callID));
                            Call call = calls[callID];
                            if (!persistantCalls.ContainsKey(callID))
                                persistantCalls.Add(callID, call);
#if DEBUG
                            CrestronConsole.PrintLine("Call.FeedbackServer_ReceivedData() Data = \r\n{0}", args.Data.ToString());
                            CrestronConsole.PrintLine("Call.FeedbackServer_ReceivedData() Call ID = {0}", callID);
                            CrestronConsole.PrintLine("Call.FeedbackServer_ReceivedData() Elements().count = {0}", args.Data.Elements().Count());
#endif
                            foreach (XElement e in args.Data.Elements())
                            {
#if DEBUG
                                CrestronConsole.PrintLine("  e.XName.LocalName = {0}", e.XName.LocalName);
#endif
                                switch (e.XName.LocalName)
                                {
                                    case "AnswerState":
                                        call.AnswerState = (CallAnswerState)Enum.Parse(typeof(CallAnswerState), e.Value, false);
                                        break;
                                    case "CallType":
                                        call.Type = (CallType)Enum.Parse(typeof(CallType), e.Value, false);
                                        break;
                                    case "CallbackNumber":
                                        call.CallbackNumber = e.Value;
                                        break;
                                    case "DeviceType":
                                        call.DeviceType = (CallDeviceType)Enum.Parse(typeof(CallDeviceType), e.Value, false);
                                        break;
                                    case "Direction":
                                        call.Direction = (CallDirection)Enum.Parse(typeof(CallDirection), e.Value, false);
                                        break;
                                    case "Duration":
                                        TimeSpan duration = TimeSpan.Parse(e.Value);
                                        call.StartTime = DateTime.Now - duration;
                                        break;
                                    case "DisplayName":
                                        call.DisplayName = e.Value;
                                        break;
                                    case "Protocol":
                                        call.Protocol = e.Value;
                                        break;
                                    case "RemoteNumber":
                                        call.RemoteNumber = e.Value;
                                        break;
                                    case "Status":
                                        call.Status = (CallStatus)Enum.Parse(typeof(CallStatus), e.Value, false);
                                        break;
                                }
                            }
#if DEBUG
                            CrestronConsole.PrintLine("OnCallStatusChange(call)");
#endif
                            OnCallStatusChange(call);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                ErrorLog.Exception("Error parsing xml in Calls.FeedbackServer_ReceivedData()", e);
            }
        }

        void Codec_HasConnected(CiscoCodec codec)
        {
#if DEBUG
            CrestronConsole.Print("Checking for calls...");
#endif
            IEnumerable<XElement> xCalls = Codec.RequestPath("Status/Call", true);

            if (xCalls != null)
            {
#if DEBUG
                CrestronConsole.PrintLine(" Call count = {0}", xCalls.Count());
#endif
                foreach (XElement xCall in xCalls)
                {
                    int callID = int.Parse(xCall.Attribute("item").Value);
                    Call call;

                    if (calls.ContainsKey(callID))
                        call = calls[callID];
                    else
                    {
                        call = new Call(Codec, callID);
                        calls.Add(callID, call);
                    }

                    foreach (XElement e in xCall.Elements())
                    {
                        switch (e.XName.LocalName)
                        {
                            case "AnswerState":
                                call.AnswerState = (CallAnswerState)Enum.Parse(typeof(CallAnswerState), e.Value, false);
                                break;
                            case "CallType":
                                call.Type = (CallType)Enum.Parse(typeof(CallType), e.Value, false);
                                break;
                            case "CallbackNumber":
                                call.CallbackNumber = e.Value;
                                break;
                            case "DeviceType":
                                call.DeviceType = (CallDeviceType)Enum.Parse(typeof(CallDeviceType), e.Value, false);
                                break;
                            case "Direction":
                                call.Direction = (CallDirection)Enum.Parse(typeof(CallDirection), e.Value, false);
                                break;
                            case "Duration":
                                TimeSpan duration = TimeSpan.Parse(e.Value);
                                call.StartTime = DateTime.Now - duration;
                                break;
                            case "DisplayName":
                                call.DisplayName = e.Value;
                                break;
                            case "Protocol":
                                call.Protocol = e.Value;
                                break;
                            case "RemoteNumber":
                                call.RemoteNumber = e.Value;
                                break;
                            case "Status":
                                call.Status = (CallStatus)Enum.Parse(typeof(CallStatus), e.Value, false);
                                break;
                        }
                    }
                    OnCallStatusChange(call);
                }
            }
            else
            {
#if DEBUG
                CrestronConsole.PrintLine(" No Calls");
#endif
            }
        }

        #region IEnumerable<Call> Members

        public IEnumerator<Call> GetEnumerator()
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

    public class CodecCallInfoChangeEventArgs : EventArgs
    {
        public CodecCallInfoChangeEventArgs(Call call)
        {
            this.Call = call;
        }

        public CodecCallInfoChangeEventArgs(Call call, bool disconnected)
        {
            this.Call = call;
            this.HasDisconnected = true;
        }
        /*
        public CodecCallInfoChangeEventArgs(Call call, string disconnectCause, CallDisconnectCauseType disconnectCauseType)
        {
            this.Call = call;
            this.HasDisconnected = true;
            this.DisconnectCause = disconnectCause;
            this.DisconnectCauseType = disconnectCauseType;
        }*/

        public Call Call;
        public bool HasDisconnected = false;
        //public string DisconnectCause;
        //public CallDisconnectCauseType DisconnectCauseType;
    }

    public delegate void CodecCallInfoChangeEventHandler(CiscoCodec codec, CodecCallInfoChangeEventArgs args);
}