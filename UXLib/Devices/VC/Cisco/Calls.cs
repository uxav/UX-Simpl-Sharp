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
        public Calls(Codec codec)
        {
            this.Codec = codec;
            this.Codec.FeedbackServer.ReceivedData += new CodecFeedbackServerReceiveEventHandler(FeedbackServer_ReceivedData);
            this.Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
        }

        Codec Codec;

        Dictionary<int, Call> calls = new Dictionary<int, Call>();

        public Call this[int callID]
        {
            get
            {
                return calls[callID];
            }
        }

        public int Count { get { return calls.Count; } }

        public int Dial(CommandArgs args)
        {
            try
            {
                if (Codec.SystemUnit.State.NumberOfActiveCalls < Codec.SystemUnit.State.MaxNumberOfActiveCalls)
                {
                    XDocument xml = Codec.SendCommand("Dial", args);
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

        public void DisconnectAll()
        {
            Codec.SendCommand("Call/DisconnectAll", new CommandArgs());
        }

        public int Disconnect()
        {
            try
            {
                Call call = calls.Last().Value;
                call.Disconnect();
                return call.ID;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Codec - Could not disconnect last call, {0}", e.Message);
                return 0;
            }
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
                CallHistoryItem callHistory = new CallHistory(Codec, 1).FirstOrDefault();
                CallStatusChange(Codec, new CodecCallInfoChangeEventArgs(
                    call, callHistory.DisconnectCause, callHistory.DisconnectCauseType));
            }
        }

        void FeedbackServer_ReceivedData(CodecFeedbackServer server, CodecFeedbackServerReceiveEventArgs args)
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
                        OnCallStatusChange(calls[callID], true);
                        calls.Remove(callID);
                    }
                    else if (!ghost)
                    {
                        if (!calls.ContainsKey(callID))
                            calls.Add(callID, new Call(Codec, callID));
                        Call call = calls[callID];
                        foreach (XElement e in args.Data.Elements())
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
                    break;
            }
        }

        void Codec_HasConnected(Codec codec)
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

        public CodecCallInfoChangeEventArgs(Call call, string disconnectCause, CallDisconnectCauseType disconnectCauseType)
        {
            this.Call = call;
            this.HasDisconnected = true;
            this.DisconnectCause = disconnectCause;
            this.DisconnectCauseType = disconnectCauseType;
        }

        public Call Call;
        public bool HasDisconnected = false;
        public string DisconnectCause;
        public CallDisconnectCauseType DisconnectCauseType;
    }

    public delegate void CodecCallInfoChangeEventHandler(Codec codec, CodecCallInfoChangeEventArgs args);
}