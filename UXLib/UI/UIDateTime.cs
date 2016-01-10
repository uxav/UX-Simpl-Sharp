using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.UI
{
    public class UIDateTime
    {
        private CTimer TimeChangeTimer;
        public DateTime TimeNow
        {
            get
            {
                return DateTime.Now;
            }
        }

        public event UIDateTimeChangeEventHandler TimeHasChanged;

        private UIDateTimeLabel DateLabel;
        private UIDateTimeLabel TimeLabel;

        public UIDateTime()
        {
            int secondsUntilNextMinute = 60 - this.TimeNow.Second;
            this.TimeChangeTimer = new CTimer(this.TimeSetup, secondsUntilNextMinute * 1000);
        }

        public UIDateTime(UIDateTimeLabel dateLabel, UIDateTimeLabel timeLabel)
        {
            this.DateLabel = dateLabel;
            this.TimeLabel = timeLabel;
            this.UpdateLabels();

            int secondsUntilNextMinute = 60 - this.TimeNow.Second;
            this.TimeChangeTimer = new CTimer(this.TimeSetup, secondsUntilNextMinute * 1000);
        }

        public void TimeSetup(object obj)
        {
            this.TimeChangeTimer.Dispose();
            this.TimeChangeTimer = new CTimer(TimeChange, null, 60000, 60000);
            if (this.TimeHasChanged != null)
                this.TimeHasChanged(this, new UIDateTimeChangeEventArgs(this.TimeNow));
            this.UpdateLabels();
        }

        public void TimeChange(object obj)
        {
            if (this.TimeHasChanged != null)
                this.TimeHasChanged(this, new UIDateTimeChangeEventArgs(this.TimeNow));

            this.UpdateLabels();
        }

        public void UpdateLabels()
        {
            if (this.DateLabel != null)
                this.DateLabel.DateTime = this.TimeNow;

            if (this.TimeLabel != null)
                this.TimeLabel.DateTime = this.TimeNow;
        }
    }

    public delegate void UIDateTimeChangeEventHandler(UIDateTime dateTime, UIDateTimeChangeEventArgs args);

    public class UIDateTimeChangeEventArgs : EventArgs
    {
        public DateTime EventTime;
        public UIDateTimeChangeEventArgs(DateTime eventTime)
            : base()
        {
            this.EventTime = eventTime;
        }
    }
}