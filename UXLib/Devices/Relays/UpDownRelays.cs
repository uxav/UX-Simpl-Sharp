using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace UXLib.Devices.Relays
{
    public class UpDownRelays
    {
        public UpDownRelays(Relay upRelay, Relay downRelay, UpDownRelayModeType modeType)
        {
            _upRelay = upRelay;
            _downRelay = downRelay;
            ModeType = modeType;
            State = UpDownRelayState.Unknown;
        }

        public UpDownRelays(Crestron.SimplSharpPro.Relay upRelay, Crestron.SimplSharpPro.Relay downRelay, UpDownRelayModeType modeType)
            : this(new Relay(upRelay), new Relay(downRelay), modeType) { }

        private readonly Relay _upRelay;
        private readonly Relay _downRelay;
        public UpDownRelayModeType ModeType { get; protected set; }
        public UpDownRelayState State { get; protected set; }
        private CTimer _waitTimer;

        public void Up()
        {
            if (_waitTimer != null)
                _waitTimer.Stop();

            if (_downRelay.State)
            {
                _downRelay.Open();
                _waitTimer = new CTimer(RelaySet, _upRelay, 500);
            }
            else
            {
                RelaySet(_upRelay);
            }

            State = UpDownRelayState.Up;
        }

        public void Down()
        {
            if (_waitTimer != null)
                _waitTimer.Stop();

            if (_upRelay.State)
            {
                _upRelay.Open();
                _waitTimer = new CTimer(RelaySet, _downRelay, 500);
            }
            else
            {
                RelaySet(_downRelay);
            }

            State = UpDownRelayState.Down;
        }

        public void Stop()
        {
            _upRelay.Open();
            _downRelay.Open();

            State = UpDownRelayState.Unknown;
        }

        public void StopUsingPulseBoth()
        {
            if (_waitTimer != null)
                _waitTimer.Stop();

            _upRelay.Close();
            _downRelay.Close();

            var timer = new CTimer(specific => Stop(), 500);
        }

        void RelaySet(object obj)
        {
            if (!(obj is Relay)) return;
            var relay = obj as Relay;
            switch (ModeType)
            {
                case UpDownRelayModeType.Momentary:
                    relay.Pulse(500);
                    break;
                case UpDownRelayModeType.Interlocked:
                    relay.Close();
                    break;
            }
        }

        public void Register()
        {
            if (_upRelay.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                ErrorLog.Error("Could not register UpDownRelays.UpRelay with ID {0}", _upRelay.CrestronRelay.ID);
            if (_downRelay.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                ErrorLog.Error("Could not register UpDownRelays.DownRelay with ID {0}", _downRelay.CrestronRelay.ID);
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