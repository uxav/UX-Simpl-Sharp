using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class Standby
    {
        internal Standby(CiscoCodec codec)
        {
            Codec = codec;
            Codec.HasConnected += new CodecConnectedEventHandler(Codec_HasConnected);
            Codec.FeedbackServer.ReceivedData += new CodecFeedbackServerReceiveEventHandler(FeedbackServer_ReceivedData);
        }

        private StandbyState _StandbyState { get; set; }

        /// <summary>
        /// Get the StandbyState of the codec
        /// </summary>
        public StandbyState StandbyState
        {
            get
            {
                return _StandbyState;
            }
        }

        public void Activate()
        {
            Codec.SendCommand("Standby/Activate");
        }

        public void Deactivate()
        {
            Codec.SendCommand("Standby/Deactivate");
        }

        /// <summary>
        /// Raised when the codec changes standby states
        /// </summary>
        public event CodecStandbyChangeEventHandler StandbyChanged;

        private CiscoCodec Codec { get; set; }

        private void OnStandbyChange(StandbyState state)
        {
            _StandbyState = state;

            try
            {
                if (StandbyChanged != null)
                    StandbyChanged(Codec, new CodecStandbyChangeEventArgs(state));
            }
            catch (Exception e)
            {
                ErrorLog.Exception("Exception thrown in CiscoCodec.StandbyChanged event handler", e);
            }
        }

        void Codec_HasConnected(CiscoCodec codec)
        {
            try
            {
                string standbyStatus = codec.RequestPath("Status/Standby").FirstOrDefault().Elements().FirstOrDefault().Value;
                _StandbyState = (StandbyState)Enum.Parse(typeof(StandbyState), standbyStatus, true);
            }
            catch (Exception e)
            {
                ErrorLog.Exception("Error in CiscoCodec.Standby.Codec_HasConnected", e);
            }
        }

        void FeedbackServer_ReceivedData(CodecFeedbackServer server, CodecFeedbackServerReceiveEventArgs args)
        {
            if (args.Path == @"Status/Standby")
            {
#if DEBUG
                CrestronConsole.PrintLine("Status for {0}", args.Path);
                CrestronConsole.PrintLine(args.Data.ToString());
#endif
                StandbyState state = (StandbyState)Enum.Parse(typeof(StandbyState), args.Data.Element("State").Value, true);
#if DEBUG
                CrestronConsole.PrintLine("state = {0}", state.ToString());
#endif
                OnStandbyChange(state);
            }
        }
    }

    /// <summary>
    /// Event handler for the codec standby change event
    /// </summary>
    /// <param name="codec">The instance of the Codec</param>
    /// <param name="StandbyActive">Current standby state</param>
    public delegate void CodecStandbyChangeEventHandler(CiscoCodec codec, CodecStandbyChangeEventArgs args);

    /// <summary>
    /// Event args for CodecStandbyChangeEventHandler
    /// </summary>
    public class CodecStandbyChangeEventArgs : EventArgs
    {
        public CodecStandbyChangeEventArgs(StandbyState state)
        {
            this.State = state;
        }

        public StandbyState State;
        public bool StandbyActive
        {
            get
            {
                if (this.State != StandbyState.Standby)
                    return false;
                else return true;
            }
        }
    }

    /// <summary>
    /// The state values for CiscoCodec standby state
    /// </summary>
    public enum StandbyState
    {
        Off,
        EnteringStandby,
        Standby
    }
}