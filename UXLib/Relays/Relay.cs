using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Relays
{
    public class Relay
    {
        public Relay(Crestron.SimplSharpPro.Relay relay)
        {
            this.CrestronRelay = relay;
#if DEBUG
		    this.CrestronRelay.StateChange += new Crestron.SimplSharpPro.RelayEventHandler(CrestronRelay_StateChange);  
#endif
            Crestron.SimplSharpPro.eDeviceRegistrationUnRegistrationResponse response = relay.Register();
            if (response != Crestron.SimplSharpPro.eDeviceRegistrationUnRegistrationResponse.Success)
            {
                ErrorLog.Error("Could not register relay with ID: {0}, {1}", relay.ID, response.ToString());
            }
        }

#if DEBUG
        void CrestronRelay_StateChange(Crestron.SimplSharpPro.Relay relay, Crestron.SimplSharpPro.RelayEventArgs args)
        {
            ErrorLog.Notice("Relay {0} = {1}", relay.ID, args.State);
        } 
#endif

        public Crestron.SimplSharpPro.Relay CrestronRelay;
        CTimer pulseTimer;

        public void Open()
        {
            if (CrestronRelay.Registered)
            {
                if (pulseTimer != null)
                    pulseTimer.Stop();
                CrestronRelay.Open();
            }
        }

        public void Close()
        {
            if (CrestronRelay.Registered)
            {
                if (pulseTimer != null)
                    pulseTimer.Stop();
                CrestronRelay.Close();
            }
        }

        public void Pulse(int time)
        {
            if (CrestronRelay.Registered)
            {
                if (!CrestronRelay.State)
                    CrestronRelay.Close();
                pulseTimer = new CTimer(pulseEnd, time);
            }
        }

        public void Pulse()
        {
            this.Pulse(500);
        }

        void pulseEnd(object obj)
        {
            CrestronRelay.Open();
        }

        public bool State
        {
            get
            {
                return CrestronRelay.State;
            }
            set
            {
                if (value)
                    this.Close();
                else
                    this.Open();
            }
        }
    }
}