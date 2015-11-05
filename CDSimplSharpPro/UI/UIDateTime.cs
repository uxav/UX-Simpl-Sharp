using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace CDSimplSharpPro.UI
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

        private UILabel DateLabel;
        private UILabel TimeLabel;

        public UIDateTime()
        {
            int secondsUntilNextMinute = 60 - this.TimeNow.Second;
            this.TimeChangeTimer = new CTimer(this.TimeSetup, secondsUntilNextMinute * 1000);
        }

        public UIDateTime(UILabel dateLabel, UILabel timeLabel)
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
            if(this.DateLabel != null)
                this.DateLabel.Text = this.TimeNow.ToString("MM/dd/yy");

            if(this.TimeLabel != null)
                this.TimeLabel.Text = this.TimeNow.ToString("HH:mm");
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