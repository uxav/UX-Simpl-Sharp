using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class SystemUnitState
    {
        public SystemUnitState(CiscoCodec codec)
        {
            Codec = codec;
            Codec.FeedbackServer.ReceivedData += new CodecFeedbackServerReceiveEventHandler(FeedbackServer_ReceivedData);
            Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
        }

        CiscoCodec Codec;

        public int MaxNumberOfActiveCalls { get; protected set; }
        public int MaxNumberOfCalls { get; protected set; }
        public int NumberOfActiveCalls { get; protected set; }
        public int NumberOfInProgressCalls { get; protected set; }
        public int NumberOfSuspendedCalls { get; protected set; }

        public event SystemUnitStateSystemChangeEventHandler SystemStateChange;

        SystemState _System;
        public SystemState System
        {
            get
            {
                return _System;
            }
            protected set
            {
                _System = value;
                if (SystemStateChange != null)
                    SystemStateChange(Codec, _System);
            }
        }

        void FeedbackServer_ReceivedData(CodecFeedbackServer server, CodecFeedbackServerReceiveEventArgs args)
        {
            switch (args.Path)
            {
                case @"Status/SystemUnit/State":
                    foreach (XElement state in args.Data.Elements())
                    {
                        switch (state.XName.LocalName)
                        {
                            case "MaxNumberOfActiveCalls": MaxNumberOfActiveCalls = int.Parse(state.Value); break;
                            case "MaxNumberOfCalls": MaxNumberOfCalls = int.Parse(state.Value); break;
                            case "NumberOfActiveCalls": NumberOfActiveCalls = int.Parse(state.Value); break;
                            case "NumberOfInProgressCalls": NumberOfInProgressCalls = int.Parse(state.Value); break;
                            case "NumberOfSuspendedCalls": NumberOfSuspendedCalls = int.Parse(state.Value); break;
                            case "System": System = (SystemState)Enum.Parse(typeof(SystemState), state.Value, false); break;
                        }
                    }
                    break;
            }
        }

        void Codec_HasConnected(CiscoCodec codec)
        {
            foreach (XElement element in Codec.RequestPath("Status/SystemUnit/State", true)
                .Elements().Where(e => !e.HasElements))
            {
#if DEBUG
                CrestronConsole.PrintLine("SystemUnit.State.{0} = {1}", element.XName.LocalName, element.Value);
#endif
                switch (element.XName.LocalName)
                {
                    case "MaxNumberOfActiveCalls": MaxNumberOfActiveCalls = int.Parse(element.Value); break;
                    case "MaxNumberOfCalls": MaxNumberOfCalls = int.Parse(element.Value); break;
                    case "NumberOfActiveCalls": NumberOfActiveCalls = int.Parse(element.Value); break;
                    case "NumberOfInProgressCalls": NumberOfInProgressCalls = int.Parse(element.Value); break;
                    case "NumberOfSuspendedCalls": NumberOfSuspendedCalls = int.Parse(element.Value); break;
                    case "System": System = (SystemState)Enum.Parse(typeof(SystemState), element.Value, false); break;
                }
            }
        }
    }

    public delegate void SystemUnitStateSystemChangeEventHandler(CiscoCodec Codec, SystemState State);

    public enum SystemState
    {
        Initializing,
        Initialized,
        InCall,
        Multisite,
        Sleeping
    }
}