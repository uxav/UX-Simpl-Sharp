using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.Polycom
{
    public class VoipLine
    {
        public VoipLine(VoipOutChannel channel, uint number)
        {
            Number = number;
            VoipOutChannel = channel;
            _CallInfoLine = new Dictionary<uint, string>();
            _CallInfoLine.Add(1, string.Empty);
            _CallInfoLine.Add(2, string.Empty);
            VoipOutChannel.Device.VoipInfoReceived += new SoundstructureVoipInfoReceivedHandler(VoipInfoReceived);
        }

        public VoipOutChannel VoipOutChannel { get; protected set; }

        void VoipInfoReceived(ISoundstructureItem item, SoundstructureVoipInfoReceivedEventArgs args)
        {
            if (item == this.VoipOutChannel)
            {
                List<string> elements = SoundstructureSocket.ElementsFromString(args.Info);
                if (elements.Count > 1)
                {
                    uint lineNumber = uint.Parse(elements[0]);
                    if (lineNumber == this.Number)
                    {
                        try
                        {
                            switch (args.Command)
                            {
                                case "voip_line_state":
                                    try
                                    {
                                        this.State = (VoipLineState)Enum.Parse(typeof(VoipLineState), elements[1], true);
                                        if (StateChanged != null)
                                            StateChanged(this, this.State);
                                    }
                                    catch (Exception e)
                                    {
                                        ErrorLog.Error("Could not parse VoipLineState \"{2}\" for Line {0}, {1}",
                                            lineNumber, e.Message, elements[1]);
                                    }
                                    break;
                                case "voip_line_label":
                                    this.Label = elements[1];
                                    break;
                                case "voip_call_appearance_line":
                                    this.CallAppearance = uint.Parse(elements[1]);
                                    break;
                                case "voip_call_appearance_state":
                                    try
                                    {
                                        VoipCallAppearanceState state = (VoipCallAppearanceState)Enum.Parse(typeof(VoipCallAppearanceState), elements[1], true);
                                        if (this.CallAppearanceState != state)
                                        {
                                            this.CallAppearanceState = state;
                                            if (CallAppearanceState == VoipCallAppearanceState.Connected)
                                                _CallConnectedTime = DateTime.Now;
                                        }
                                        try
                                        {
                                            if (CallAppearanceStateChanged != null)
                                                CallAppearanceStateChanged(this, new VoipLineCallAppearanceStateEventArgs(this.CallAppearance, this.CallAppearanceState));
                                        }
                                        catch (Exception e)
                                        {
                                            ErrorLog.Exception(string.Format("Error calling event {0}.CallAppearanceStateChanged", this.GetType().Name), e);
                                        }
                                    }
                                    catch(Exception e)
                                    {
                                        ErrorLog.Error("Could not parse VoipCallAppearanceState \"{0}\" for Line {1}, {2}", elements[1], lineNumber, e.Message);
                                    }
                                    break;
                                case "voip_call_appearance_info":
                                    if (elements.Count > 3)
                                    {
                                        uint lineIndex = uint.Parse(elements[1]);
                                        _CallInfoLine[lineIndex] = elements[2];
                                        if (CallInfoLineChanged != null)
                                            CallInfoLineChanged(this, new VoipLineCallInfoLineEventArgs(lineIndex, elements[2]));
                                    }
                                    break;
                            }
                        }
                        catch (Exception e)
                        {
                            ErrorLog.Error("Error parsing Voip feedback info in VoipLine[{0}], {1}", this.Number, e.Message);
                            ErrorLog.Error("VoipInfoReceived() args.Command = \"{0}\" args.Info = \"{1}\"", args.Command, args.Info);
                        }
                    }
                }
            }
        }

        public uint Number { get; protected set; }

        public VoipLineState State { get; protected set; }

        public event VoipLineStateEventHandler StateChanged;

        public uint CallAppearance { get; protected set; }

        public string Label { get; protected set; }

        private DateTime _CallConnectedTime;
        public TimeSpan CallTimer
        {
            get
            {
                if (_CallConnectedTime != null)
                    return DateTime.Now - _CallConnectedTime;
                else return TimeSpan.FromSeconds(0);
            }
        }

        public VoipCallAppearanceState CallAppearanceState { get; protected set; }

        public event VoipLineCallAppearanceStateEventHandler CallAppearanceStateChanged;

        public bool Registered
        {
            get
            {
                if (this.State == VoipLineState.Line_Not_Registered) return false;
                return true;
            }
        }

        Dictionary<uint, string> _CallInfoLine { get; set; }
        public ReadOnlyDictionary<uint, string> CallInfoLine
        {
            get
            {
                return new ReadOnlyDictionary<uint, string>(_CallInfoLine);
            }
        }

        public event VoipLineCallInfoLineEventHandler CallInfoLineChanged;
    }

    public delegate void VoipLineStateEventHandler(VoipLine line, VoipLineState state);

    public delegate void VoipLineCallAppearanceStateEventHandler(VoipLine line, VoipLineCallAppearanceStateEventArgs args);

    public class VoipLineCallAppearanceStateEventArgs : EventArgs
    {
        public VoipLineCallAppearanceStateEventArgs(uint callAppearance, VoipCallAppearanceState state)
        {
            CallAppearance = callAppearance;
            State = state;
        }

        public uint CallAppearance;
        public VoipCallAppearanceState State;
    }

    public delegate void VoipLineCallInfoLineEventHandler(VoipLine line, VoipLineCallInfoLineEventArgs args);

    public class VoipLineCallInfoLineEventArgs
    {
        public VoipLineCallInfoLineEventArgs(uint lineIndex, string value)
        {
            Index = lineIndex;
            Value = value;
        }

        public uint Index;
        public string Value;

    }

    public enum VoipCallAppearanceState
    {
        Free,
        DialTone,
        Setup,
        Proceeding,
        Offering,
        Ringback,
        Ncas_Call_Hold,
        Disconnected,
        Connected
    }

    public enum VoipLineState
    {
        None,
        Line_Not_Registered,
        Line_Registered,
        Proceed,
        Offering,
        Call_Active,
        Conference,
        Call_On_Hold,
        Secure_RTP
    }
}