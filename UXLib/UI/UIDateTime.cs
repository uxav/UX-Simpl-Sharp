using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.UI
{
    public class UIDateTime
    {
        public UIDateTime()
        {
            int secondsUntilNextMinute = 60 - this.TimeNow.Second;
            this.timeChangeTimer = new CTimer(timeSetup, secondsUntilNextMinute * 1000);
        }

        public UIDateTime(UIDateTimeLabel dateLabel, UIDateTimeLabel timeLabel)
            : this()
        {
            this.DateLabel = dateLabel;
            this.TimeLabel = timeLabel;
            this.UpdateLabels();
        }

        CTimer timeChangeTimer;

        public event UIDateTimeChangeEventHandler TimeHasChanged;

        public UIDateTimeLabel DateLabel { get; protected set; }
        public UIDateTimeLabel TimeLabel { get; protected set; }

        void timeSetup(object obj)
        {
            this.timeChangeTimer.Dispose();
            this.timeChangeTimer = new CTimer(timeChange, null, 60000, 60000);
            if (this.TimeHasChanged != null)
                this.TimeHasChanged(this, new UIDateTimeChangeEventArgs(this.TimeNow));
            this.UpdateLabels();
        }

        void timeChange(object obj)
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

        public DateTime TimeNow
        {
            get
            {
                return DateTime.Now;
            }
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