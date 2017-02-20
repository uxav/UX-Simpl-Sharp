using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Models;

namespace UXLib.Devices.Audio.Revolabs
{
    public class RevolabMic : IVolumeDevice
    {
        internal RevolabMic(RevolabsExecutiveElite controller, uint channelNumber)
        {
            this.Controller = controller;
            this.ChannelNumber = channelNumber;
            this.Controller.Send(string.Format("get mictype ch {0}", channelNumber));
            this.Controller.Send(string.Format("get mutestatus ch {0}", channelNumber));
            this.Controller.Send(string.Format("get micgroup ch {0}", channelNumber));
#if DEBUG
            CrestronConsole.PrintLine("Created mic at channel index {0}", channelNumber);
#endif
        }

        public RevolabsExecutiveElite Controller { get; protected set; }
        public uint ChannelNumber { get; protected set; }

        MicStatus _Status;
        public MicStatus Status
        {
            get { return _Status; }
            protected set
            {
                if (_Status != value)
                {
                    _Status = value;
#if DEBUG
                    CrestronConsole.PrintLine("Mic {0} Status = {1}", this.ChannelNumber, value);
#endif
                }
            }
        }

        public MicType Type { get; protected set; }

        internal void UpdateValue(string command, string value)
        {
            switch (command)
            {
                case "micstatus":
                    try
                    {
                        this.Status = (MicStatus)uint.Parse(value);
                    }
                    catch (Exception e)
                    {
                        ErrorLog.Error("{0}.UpdateValue() Could not parse micstatus value \"{1}\", {2}", this.GetType().Name, value, e.Message);
                    }
                    break;
                case "mictype":
                    try
                    {
                        this.Type = (MicType)uint.Parse(value);
                    }
                    catch (Exception e)
                    {
                        ErrorLog.Error("{0}.UpdateValue() Could not parse mictype value \"{1}\", {2}", this.GetType().Name, value, e.Message);
                    }
#if DEBUG
                    CrestronConsole.PrintLine("Mic {0} is MicType.{1}", this.ChannelNumber, this.Type);
#endif
                    break;
                case "mutestatus":
                    try
                    {
                        string[] args = value.Split(',');
                        _Mute = (args[0] == "1") ? true : false;
                        _MuteType = (MuteType)uint.Parse(args[1]);
                        _MuteLock = (args[2] == "1") ? true : false;
                    }
                    catch (Exception e)
                    {
                        ErrorLog.Error("{0}.UpdateValue() Could not parse mutestatus value \"{1}\", {2}", this.GetType().Name, value, e.Message);                        
                    }
#if DEBUG
                    CrestronConsole.PrintLine("Mic {0} Mute = {1}, MuteType.{2}, MuteLock = {3}", this.ChannelNumber, this.Mute, this.MuteType, this.MuteLock);
#endif
                    if (VolumeChanged != null)
                    {
                        try
                        {
                            VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.MuteChanged));
                        }
                        catch (Exception e)
                        {
                            ErrorLog.Error("{0}.VolumeChanged event error, {1}", this.GetType().Name, e.Message);                            
                        }
                    }
                    break;
            }
        }

        internal void NotifyValue(string command, string value)
        {
            switch (command)
            {
                case "status_change_mic":
                    this.Status = (MicStatus)uint.Parse(value);
                    break;
                case "set_mute_status":
                    string[] args = value.Split(',');
                    _Mute = (args[0] == "1") ? true : false;
                    _MuteType = (MuteType)uint.Parse(args[1]);
                    _MuteLock = (args[2] == "1") ? true : false;
#if DEBUG
                    CrestronConsole.PrintLine("Mic {0} Mute = {1}, MuteType.{2}, MuteLock = {3}", this.ChannelNumber, this.Mute, this.MuteType, this.MuteLock);
#endif
                    if (VolumeChanged != null)
                        VolumeChanged(this, new VolumeChangeEventArgs(VolumeLevelChangeEventType.MuteChanged));
                    break;
            }
        }

        bool _Mute;
        public bool Mute
        {
            get { return _Mute; }
            set
            {
                this.Controller.Send(string.Format("set mute ch {0} {1}", this.ChannelNumber, value ? 1 : 0));
            }
        }

        MuteType _MuteType;
        public MuteType MuteType
        {
            get { return _MuteType; }
            set
            {
                throw new NotImplementedException("Not supported as yet");
            }
        }

        bool _MuteLock;
        public bool MuteLock
        {
            get { return _MuteLock; }
            set
            {
                throw new NotImplementedException("Not supported as yet");
            }
        }

        #region IVolumeDevice Members

        public string Name
        {
            get { return string.Format("Mic {0}", this.ChannelNumber); }
        }

        public ushort VolumeLevel
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool VolumeMute
        {
            get
            {
                return this.Mute;
            }
            set
            {
                this.Mute = value;
            }
        }

        public bool SupportsVolumeMute
        {
            get { return true; }
        }

        public bool SupportsVolumeLevel
        {
            get { return false; }
        }

        public event VolumeDeviceChangeEventHandler VolumeChanged;

        #endregion
    }

    public enum MicStatus
    {
        Off = 0,
        On = 1,
        Standby = 2,
        Charging = 3,
        OutOfRange = 4,
        NotPaired = 5,
        Pairing = 6,
        UnPairing = 7,
        Updating = 8
    }

    public enum MicType
    {
        Wearable = 0,
        Omni = 1,
        Directional = 2,
        XLR = 3,
        MiniXLR = 4,
        Gooseneck = 5,
        Handheld = 10,
        Unknown = 11
    }

    public enum MuteType
    {
        Individual = 0,
        TableTop = 1,
        PushToTalk = 2
    }
}