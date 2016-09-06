using System;
using System.Linq;
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
        internal Calls(CiscoCodec codec)
        {
            this.Codec = codec;
            this._Calls = new Dictionary<int, Call>();
            this.Codec.FeedbackServer.ReceivedData += new CodecFeedbackServerReceiveEventHandler(FeedbackServer_ReceivedData);
            this.Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
        }

        CiscoCodec Codec;

        Dictionary<int, Call> _Calls { get; set; }

        public Call this[int callID]
        {
            get
            {
                return _Calls[callID];
            }
        }

        /// <summary>
        /// Get the count of active calls (Call.Status != CallStatus.Idle)
        /// </summary>
        public int Count { get { return _Calls.Count(c => c.Value.Status != CallStatus.Idle); } }

        /// <summary>
        /// Get the total count of calls in the log
        /// </summary>
        public int CountLog { get { return _Calls.Count; } }

        /// <summary>
        /// Get the calls which are in some form of active state
        /// </summary>
        public IEnumerable<Call> Active
        {
            get
            {
                return this.Where(c => c.Status != CallStatus.Idle);
            }
        }

        /// <summary>
        /// Get the calls which are dialling or connecting
        /// </summary>
        public IEnumerable<Call> InProgress
        {
            get
            {
                return this.Where(c => c.Status == CallStatus.Connecting || c.Status == CallStatus.Dialling);
            }
        }

        /// <summary>
        /// Get the calls which are connected
        /// </summary>
        public IEnumerable<Call> Connected
        {
            get
            {
                return this.Where(c => c.Status == CallStatus.Connected);
            }
        }

        /// <summary>
        /// Get the calls which are connected or on hold
        /// </summary>
        public IEnumerable<Call> ConnectedOrHeld
        {
            get
            {
                return this.Where(c => c.Status == CallStatus.Connected || c.Status == CallStatus.OnHold);
            }
        }

        public DialResult Dial(CommandArgs args)
        {
            try
            {
                XDocument xml = Codec.SendCommand("Dial", args);
                XElement dialResult = xml.Root.Element("DialResult");
                if (dialResult.Attribute("status").Value == "OK")
                {
                    int callID = int.Parse(dialResult.Element("CallId").Value);
                    if (!_Calls.ContainsKey(callID))
                    {
                        _Calls.Add(callID, new Call(Codec, callID));
                        if (args.ContainsArg("CallType"))
                            _Calls[callID].Type = (CallType)Enum.Parse(typeof(CallType), args["CallType"].Value, false);
                        _Calls[callID].RemoteNumber = args["Number"].Value;
                        _Calls[callID].Direction = CallDirection.Outgoing;
                        _Calls[callID].Status = CallStatus.Dialling;
                        OnCallStatusChange(_Calls[callID]);
                    }
                    return new DialResult(callID);
                }   
                else if(dialResult.Attribute("status").Value == "Error")
                {
                    return new DialResult(int.Parse(dialResult.Element("Cause").Value), dialResult.Element("Description").Value);
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in Codec.Dial(), {0}", e.Message);
            }

            return new DialResult(0, "Unknown Error");
        }

        public DialResult Dial(string number)
        {
            CommandArgs args = new CommandArgs("Number", number);
            return Dial(args);
        }

        public DialResult Dial(string number, CallType callType)
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
            if (_Calls.ContainsKey(callID))
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

                        if (ghost && _Calls.ContainsKey(callID))
                        {
                            if (_Calls[callID].Status != CallStatus.Idle)
                                _Calls[callID].Status = CallStatus.Idle;
                            Call disconnectedCall = _Calls[callID];
                            OnCallStatusChange(disconnectedCall, true);
                        }
                        else if (!ghost)
                        {
                            if (!_Calls.ContainsKey(callID))
                                _Calls.Add(callID, new Call(Codec, callID));
                            Call call = _Calls[callID];
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

        private void GetCalls()
        {

#if DEBUG
            CrestronConsole.Print("Checking for calls...");
#endif
            IEnumerable<XElement> xCalls = Codec.RequestPath("Status/Call");

            if (xCalls != null)
            {
#if DEBUG
                CrestronConsole.PrintLine(" Call count = {0}", xCalls.Count());
#endif
                foreach (XElement xCall in xCalls)
                {
                    int callID = int.Parse(xCall.Attribute("item").Value);
                    Call call;

                    if (_Calls.ContainsKey(callID))
                        call = _Calls[callID];
                    else
                    {
                        call = new Call(Codec, callID);
                        _Calls.Add(callID, call);
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

        void Codec_HasConnected(CiscoCodec codec)
        {
            this.GetCalls();
        }

        #region IEnumerable<Call> Members

        public IEnumerator<Call> GetEnumerator()
        {
            return _Calls.Values.GetEnumerator();
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
        internal CodecCallInfoChangeEventArgs(Call call)
        {
            this.Call = call;
        }

        internal CodecCallInfoChangeEventArgs(Call call, bool disconnected)
        {
            this.Call = call;
            this.HasDisconnected = true;
        }

        public Call Call;
        public bool HasDisconnected = false;
    }

    public delegate void CodecCallInfoChangeEventHandler(CiscoCodec codec, CodecCallInfoChangeEventArgs args);
}