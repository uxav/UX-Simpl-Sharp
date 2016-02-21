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
        }

        public UpDownRelays(Crestron.SimplSharpPro.Relay upRelay, Crestron.SimplSharpPro.Relay downRelay, UpDownRelayModeType modeType)
        {
            UpRelay = new UXLib.Relays.Relay(upRelay);
            DownRelay = new UXLib.Relays.Relay(downRelay);
            this.ModeType = modeType;
        }

        UXLib.Relays.Relay UpRelay;
        UXLib.Relays.Relay DownRelay;
        public UpDownRelayModeType ModeType { get; protected set; }
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
    }

    public enum UpDownRelayModeType
    {
        Momentary,
        Interlocked
    }
}