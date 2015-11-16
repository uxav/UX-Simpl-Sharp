using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro.UI
{
    public class UITimeOut
    {
        BasicTriList Device;
        public int TimeOutInSeconds;
        private CTimer TimeOutTimer;
        public object TimeOutObject;

        public event UITimeOutEventHandler TimedOut;

        public UITimeOut(object timeOutObject, int timeOutInSeconds, BasicTriList device)
        {
            this.TimeOutObject = timeOutObject;
            this.TimeOutInSeconds = timeOutInSeconds;
            Device = device;
            Device.SigChange += new SigEventHandler(Device_SigChange);
        }

        public void Set()
        {
            if (this.TimeOutTimer == null || this.TimeOutTimer.Disposed)
                this.TimeOutTimer = new CTimer(this.TimeOut, this.TimeOutInSeconds * 1000);
        }

        public void Reset()
        {
            if (this.TimeOutTimer != null && !this.TimeOutTimer.Disposed)
            {
                this.TimeOutTimer.Dispose();
                this.TimeOutTimer = new CTimer(this.TimeOut, this.TimeOutInSeconds * 1000);
            }
        }

        public void Cancel()
        {
            if (this.TimeOutTimer != null)
            {
                this.TimeOutTimer.Stop();
                this.TimeOutTimer.Dispose();
            }
        }

        public void TimeOut(object obj)
        {
            if (this.TimedOut != null && !this.TimeOutTimer.Disposed)
            {
                this.TimeOutTimer.Dispose();
                this.TimedOut(this.TimeOutObject, new UITimeOutEventArgs());
            }
        }

        void Device_SigChange(BasicTriList currentDevice, Crestron.SimplSharpPro.SigEventArgs args)
        {
            this.Reset();
        }

        public void Dispose()
        {
            this.TimeOutTimer.Dispose();
            this.TimeOutTimer = null;
            Device.SigChange -= new SigEventHandler(Device_SigChange);
        }
    }

    public delegate void UITimeOutEventHandler(object timeOutObject, UITimeOutEventArgs args);

    public class UITimeOutEventArgs : EventArgs
    {
        public UITimeOutEventArgs()
            : base()
        {
            
        }
    }
}