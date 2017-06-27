using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;
using Crestron.SimplSharpPro.CrestronThread;

namespace UXLib.Devices.VC.Cisco
{
    public class Calls : IEnumerable<Call>
    {
        internal Calls(CiscoCodec codec)
        {
            this.Codec = codec;
            this._Calls = new Dictionary<int, Call>();
            this.Codec.FeedbackServer.ReceivedData += new CodecFeedbackServerReceiveEventHandler(FeedbackServer_ReceivedData);
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
                return this.Where(c => c.Status != CallStatus.Idle && !c.Ghost);
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
                        OnCallStatusChange(_Calls[callID], CallInfoChangeNotificationSource.DialRequest);
                    }
                    try
                    {
                        if (this.Codec.LoggingEnabled)
                        {
                            this.Codec.Logger.Log("Dialed call with API - Result ok - Call ID: {0}", callID);
                        }
                    }
                    catch
                    {
                        ErrorLog.Error("Could not write to Codec.Logger in Codec.Call.Dial()");
                    }
                    return new DialResult(callID);
                }   
                else if(dialResult.Attribute("status").Value == "Error")
                {
                    if (this.Codec.LoggingEnabled)
                    {
                        this.Codec.Logger.Log("Dialed call with API - Result error - {0}", dialResult.Element("Description").Value);
                    }
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

        void OnCallStatusChange(Call call, CallInfoChangeNotificationSource source)
        {
            try
            {
                if (CallStatusChange != null && !call.Ghost)
                    CallStatusChange(Codec, new CodecCallInfoChangeEventArgs(call, source));
            }
            catch(Exception e)
            {
                ErrorLog.Exception("Error calling event at OnCallStatusChange", e);
            }

#if DEBUG
            string statusInfo = string.Format("Call ID {0} Status Updated from {3}, Name: {1} Status: {2}",
                call.ID, call.DisplayName, call.Status, source);
            CrestronConsole.PrintLine(statusInfo);
            ErrorLog.Notice(statusInfo);
#endif

            this.Codec.FusionUpdate();
        }

        void FeedbackServer_ReceivedData(CodecFeedbackServer server, CodecFeedbackServerReceiveEventArgs args)
        {
            try
            {
                switch (args.Path)
                {
                    case @"Status/Conference/Call":
                        int confCallID = int.Parse(args.Data.Attribute("item").Value);
                        if (_Calls.ContainsKey(confCallID) &&
                            (_Calls[confCallID].Status == CallStatus.Connecting || _Calls[confCallID].Status == CallStatus.Ringing))
                        {
                            //CrestronConsole.PrintLine("Received conference status for call {0} which is currently shown as connecting... Requesting full update", confCallID);
                            //this.Update();
                        }
                        break;
                    case @"Status/Call":
                        int callID = int.Parse(args.Data.Attribute("item").Value);
                        bool ghost = args.Data.Attribute("ghost") != null ? bool.Parse(args.Data.Attribute("ghost").Value) : false;

                        if (ghost && _Calls.ContainsKey(callID))
                        {
                            Call disconnectedCall = _Calls[callID];
                            if (disconnectedCall.Status != CallStatus.Idle)
                            {
                                disconnectedCall.Status = CallStatus.Idle;
                                OnCallStatusChange(disconnectedCall, CallInfoChangeNotificationSource.HttpFeedbackServer);
                            }
                            disconnectedCall.Ghost = true;
                        }
                        else if (!ghost)
                        {
                            if (!_Calls.ContainsKey(callID))
                                _Calls.Add(callID, new Call(Codec, callID));
                            Call call = _Calls[callID];

                            foreach (XElement e in args.Data.Elements())
                            {
#if DEBUG
                                if (!e.HasElements)
                                    CrestronConsole.PrintLine("Codec.Calls[{0}].{1} = {2}", call.ID, e.XName.LocalName, e.Value);
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
                                        if (e.Value.Length > 0)
                                            call.RemoteNumber = e.Value;
                                        break;
                                    case "Status":
                                        //CrestronConsole.PrintLine("Codec RX - Call {0} status = {1}", call.ID, e.Value);
                                        if (Codec.LoggingEnabled)
                                        {
                                            Codec.Logger.Log("Codec received call Status for Call {0} - Status: {1}", call.ID, e.Value);
                                        }

                                        try
                                        {
                                            CallStatus _Status = (CallStatus)Enum.Parse(typeof(CallStatus), e.Value, false);
                                            if (_Status != call.Status && !(_Status == CallStatus.Disconnecting && call.Status == CallStatus.Idle))
                                            {
                                                call.Status = _Status;
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            ErrorLog.Error("Could not parse Enum CallStatus from value {0}, {1}", e.Value, ex.Message);
                                        }
                                        break;
                                }
                            }
                            OnCallStatusChange(call, CallInfoChangeNotificationSource.HttpFeedbackServer);
                        }
                        break;
                }
            }
            catch (Exception e)
            {
                ErrorLog.Exception("Error parsing xml in Calls.FeedbackServer_ReceivedData()", e);
            }
        }

        /// <summary>
        /// Perform a full call status update of the codec
        /// </summary>
        public void Update()
        {
            Thread nt = new Thread(UpdateCallsThread, null, Thread.eThreadStartOptions.CreateSuspended);
            nt.Priority = Thread.eThreadPriority.UberPriority;
            nt.Start();
        }

        object UpdateCallsThread(object obj)
        {
            Stopwatch sw = new Stopwatch();
            sw.Start();
#if DEBUG
            CrestronConsole.Print("Checking for calls...");
#endif
            IEnumerable<XElement> xCalls = Codec.RequestPath("Status/Call");

            if (xCalls != null)
            {
#if DEBUG
                CrestronConsole.PrintLine(" Call count = {0}", xCalls.Count());
#endif
                Dictionary<int, Call> receivedCalls = new Dictionary<int, Call>();

                foreach (XElement xCall in xCalls)
                {
                    bool statusChanged = false;
                    int callID = int.Parse(xCall.Attribute("item").Value);
                    Call call;

                    if (_Calls.ContainsKey(callID))
                        call = _Calls[callID];
                    else
                    {
                        call = new Call(Codec, callID);
                        _Calls.Add(callID, call);
                    }

                    receivedCalls.Add(call.ID, call);

                    foreach (XElement e in xCall.Elements())
                    {
                        switch (e.XName.LocalName)
                        {
                            case "AnswerState":
                                CallAnswerState _AnswerState = (CallAnswerState)Enum.Parse(typeof(CallAnswerState), e.Value, false);
                                if (_AnswerState != call.AnswerState)
                                {
                                    call.AnswerState = _AnswerState;
                                    statusChanged = true;
                                }
                                break;
                            case "CallType":
                                CallType _Type = (CallType)Enum.Parse(typeof(CallType), e.Value, false);
                                if (_Type != call.Type)
                                {
                                    call.Type = _Type;
                                    statusChanged = true;
                                }
                                break;
                            case "CallbackNumber":
                                call.CallbackNumber = e.Value;
                                if (call.CallbackNumber != e.Value)
                                {
                                    call.CallbackNumber = e.Value;
                                    statusChanged = true;
                                }
                                break;
                            case "DeviceType":
                                CallDeviceType _DeviceType = (CallDeviceType)Enum.Parse(typeof(CallDeviceType), e.Value, false);
                                if (call.DeviceType != _DeviceType)
                                {
                                    call.DeviceType = _DeviceType;
                                    statusChanged = true;
                                }
                                break;
                            case "Direction":
                                CallDirection _Direction = (CallDirection)Enum.Parse(typeof(CallDirection), e.Value, false);
                                if (call.Direction != _Direction)
                                {
                                    call.Direction = _Direction;
                                    statusChanged = true;
                                }
                                break;
                            case "Duration":
                                TimeSpan duration = TimeSpan.Parse(e.Value);
                                call.StartTime = DateTime.Now - duration;
                                break;
                            case "DisplayName":
                                call.DisplayName = e.Value;
                                if (call.DisplayName != e.Value)
                                {
                                    call.DisplayName = e.Value;
                                    statusChanged = true;
                                }
                                break;
                            case "Protocol":
                                call.Protocol = e.Value;
                                if (call.Protocol != e.Value)
                                {
                                    call.Protocol = e.Value;
                                    statusChanged = true;
                                }
                                break;
                            case "RemoteNumber":
                                call.RemoteNumber = e.Value;
                                if (call.RemoteNumber != e.Value)
                                {
                                    call.RemoteNumber = e.Value;
                                    statusChanged = true;
                                }
                                break;
                            case "Status":
                                CallStatus _Status = (CallStatus)Enum.Parse(typeof(CallStatus), e.Value, false);
                                if (_Status != call.Status && !(_Status == CallStatus.Disconnecting && call.Status == CallStatus.Idle))
                                {
                                    call.Status = _Status;
                                    statusChanged = true;
                                }
                                break;
                        }
                    }
                    if (statusChanged)
                        OnCallStatusChange(call, CallInfoChangeNotificationSource.UpdateRequest);
                }

                foreach (Call call in this.Active)
                {
                    if (!receivedCalls.ContainsKey(call.ID))
                    {
                        call.Status = CallStatus.Idle;
                        OnCallStatusChange(call, CallInfoChangeNotificationSource.UpdateRequest);
                        call.Ghost = true;

                        if (this.Codec.LoggingEnabled)
                        {
                            this.Codec.Logger.Log("Call id {0} was found in Codec.Calls but not present in an Update Request, Call was removed and notified as idle", call.ID);
                        }
                    }
                }
            }
            else
            {
#if DEBUG
                CrestronConsole.PrintLine(" No Calls");
#endif
            }

            return null;
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
        internal CodecCallInfoChangeEventArgs(Call call, CallInfoChangeNotificationSource source)
        {
            this.Call = call;
            this.NotificationSource = source;
        }

        public Call Call;
        CallInfoChangeNotificationSource NotificationSource;
    }

    public enum CallInfoChangeNotificationSource
    {
        HttpFeedbackServer,
        UpdateRequest,
        DialRequest
    }

    public delegate void CodecCallInfoChangeEventHandler(CiscoCodec codec, CodecCallInfoChangeEventArgs args);
}