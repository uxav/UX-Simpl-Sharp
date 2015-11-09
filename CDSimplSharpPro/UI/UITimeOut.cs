using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CDSimplSharpPro.UI
{
    public class UITimeOut
    {
        public UIButtonCollection ResetTimeOutButtonGroup;
        public int TimeOutInSeconds;
        private CTimer TimeOutTimer;
        public object TimeOutObject;

        public event UITimeOutEventHandler TimedOut;

        public UITimeOut(object timeOutObject, int timeOutInSeconds, UIButtonCollection resetTimeOutButtonGroup)
        {
            this.TimeOutObject = timeOutObject;
            this.TimeOutInSeconds = timeOutInSeconds;
            this.ResetTimeOutButtonGroup = resetTimeOutButtonGroup;
            this.ResetTimeOutButtonGroup.ButtonEvent += new UIButtonCollectionEventHandler(ResetTimeOutButtonGroup_ButtonEvent);
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

        void ResetTimeOutButtonGroup_ButtonEvent(UIButtonCollection group, UIButton button, UIButtonEventArgs args)
        {
            if(args.EventType == eUIButtonEventType.Pressed || args.EventType == eUIButtonEventType.Released)
                this.Reset();
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