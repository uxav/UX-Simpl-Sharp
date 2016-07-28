using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Relays
{
    public class UpDownRelays
    {
        public UpDownRelays(UXLib.Relays.Relay upRelay, UXLib.Relays.Relay downRelay, UpDownRelayModeType modeType)
        {
            UpRelay = upRelay;
            DownRelay = downRelay;
            this.ModeType = modeType;
            this.State = UpDownRelayState.Unknown;
        }

        public UpDownRelays(Crestron.SimplSharpPro.Relay upRelay, Crestron.SimplSharpPro.Relay downRelay, UpDownRelayModeType modeType)
            : this(new UXLib.Relays.Relay(upRelay), new UXLib.Relays.Relay(downRelay), modeType) { }

        UXLib.Relays.Relay UpRelay;
        UXLib.Relays.Relay DownRelay;
        public UpDownRelayModeType ModeType { get; protected set; }
        public UpDownRelayState State { get; protected set; }
        CTimer waitTimer;

        public void Up()
        {
            if (waitTimer != null)
                waitTimer.Stop();

            if (DownRelay.State)
            {
                DownRelay.Open();
                waitTimer = new CTimer(RelaySet, UpRelay, 500);
            }
            else
            {
                RelaySet(UpRelay);
            }

            this.State = UpDownRelayState.Up;
        }

        public void Down()
        {
            if (waitTimer != null)
                waitTimer.Stop();

            if (UpRelay.State)
            {
                UpRelay.Open();
                waitTimer = new CTimer(RelaySet, DownRelay, 500);
            }
            else
            {
                RelaySet(DownRelay);
            }

            this.State = UpDownRelayState.Down;
        }

        public void Stop()
        {
            UpRelay.Open();
            DownRelay.Open();

            this.State = UpDownRelayState.Unknown;
        }

        void RelaySet(object obj)
        {
            if (obj is UXLib.Relays.Relay)
            {
                UXLib.Relays.Relay relay = obj as UXLib.Relays.Relay;
                switch (this.ModeType)
                {
                    case UpDownRelayModeType.Momentary:
                        relay.Pulse(500);
                        break;
                    case UpDownRelayModeType.Interlocked:
                        relay.Close();
                        break;
                }
            }
        }

        public void Register()
        {
            if (this.UpRelay.Register() != Crestron.SimplSharpPro.eDeviceRegistrationUnRegistrationResponse.Success)
                ErrorLog.Error("Could not register UpDownRelays.UpRelay with ID {0}", UpRelay.CrestronRelay.ID);
            if (this.DownRelay.Register() != Crestron.SimplSharpPro.eDeviceRegistrationUnRegistrationResponse.Success)
                ErrorLog.Error("Could not register UpDownRelays.DownRelay with ID {0}", DownRelay.CrestronRelay.ID);
        }
    }

    public enum UpDownRelayModeType
    {
        Momentary,
        Interlocked
    }

    public enum UpDownRelayState
    {
        Up,
        Down,
        Unknown
    }
}