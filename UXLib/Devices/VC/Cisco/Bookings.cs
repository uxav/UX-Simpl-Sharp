using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Devices.VC.Cisco
{
    public class Bookings
    {
        private readonly CiscoCodec _codec;

        internal Bookings(CiscoCodec codec)
        {
            _codec = codec;
        }

        public BookingResults List()
        {
            var args = new CommandArgs("Days", 1) {{"DayOffset", 0}};
            var xml = Codec.SendCommand("Bookings/List", args);
            var element = xml.Root.Element("BookingsListResult");
#if DEBUG
            CrestronConsole.PrintLine(element.ToString());
#endif
            return new BookingResults(_codec, element);
        }

        public CiscoCodec Codec
        {
            get { return _codec; }
        }
    }

    public class BookingResults : IEnumerable<Booking>
    {
        private readonly CiscoCodec _codec;
        private readonly List<Booking> _bookings = new List<Booking>();

        internal BookingResults(CiscoCodec codec, XElement element)
        {
            _codec = codec;
            if (element.Attribute("status").Value == "OK")
            {
                var results = element.Elements("Booking");
                foreach (var result in results)
                {
                    _bookings.Add(new Booking(_codec, result));
                }
            }
        }

        public Booking CurrentBooking
        {
            get { return _bookings.FirstOrDefault(b => b.Time.IsCurrent); }
        }

        public IEnumerator<Booking> GetEnumerator()
        {
            return _bookings.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class Booking
    {
        private readonly CiscoCodec _codec;

        internal Booking(CiscoCodec codec, XElement element)
        {
            _codec = codec;
            BookingId = element.Element("Id").Value;
            Title = element.Element("Title").Value;
            Time = new BookingTime(element.Element("Time"));
            Privacy = element.Element("Privacy").Value;
            BookingStatus = element.Element("BookingStatus").Value;
            DialInfo = new BookingDialInfo(element.Element("DialInfo"));
        }

        public string BookingId { get; private set; }

        public string Title { get; private set; }

        public BookingTime Time { get; private set; }

        public string Privacy { get; private set; }

        public string BookingStatus { get; private set; }

        public BookingDialInfo DialInfo { get; private set; }

        public bool CanJoin
        {
            get
            {
                return DateTime.Now >= Time.CanJoinFrom.ToLocalTime() &&
                       DialInfo.Calls.Call.Any(c => !string.IsNullOrEmpty(c.Number)) && !Time.HasEnded;
            }
        }
    }

    public class BookingTime
    {
        internal BookingTime(XElement element)
        {
            StartTime = DateTime.Parse(element.Element("StartTime").Value);
            EndTime = DateTime.Parse(element.Element("EndTime").Value);
            StartTimeBuffer = int.Parse(element.Element("StartTimeBuffer").Value);
            EndTimeBuffer = int.Parse(element.Element("EndTimeBuffer").Value);
        }

        public DateTime StartTime { get; private set; }
        public DateTime EndTime { get; private set; }
        public int StartTimeBuffer { get; private set; }
        public int EndTimeBuffer { get; private set; }

        public DateTime CanJoinFrom
        {
            get { return StartTime - TimeSpan.FromSeconds(StartTimeBuffer); }
        }

        public bool IsCurrent
        {
            get { return DateTime.Now >= StartTime.ToLocalTime() && DateTime.Now <= EndTime.ToLocalTime(); }
        }

        public bool IsUpcoming
        {
            get { return DateTime.Now <= StartTime.ToLocalTime(); }
        }

        public bool HasEnded
        {
            get { return DateTime.Now >= EndTime.ToLocalTime() + TimeSpan.FromSeconds(EndTimeBufferInSeconds); }
        }

        public override string ToString()
        {
            var st = StartTime.ToLocalTime();
            var et = StartTime.ToLocalTime();
            return string.Format("{0} {1} - {2}", st.ToString("ddd"), st.ToString("t"), et.ToString("t"));
        }
    }

    public class BookingDialInfo
    {
        internal BookingDialInfo(XElement element)
        {
            ConnectMode = element.Element("ConnectMode").Value;
            Calls = new BookingCalls(element.Element("Calls"));
        }

        public string ConnectMode { get; private set; }
        public BookingCalls Calls { get; private set; }
    }

    public class BookingCalls : IEnumerable<BookingCallInfo>
    {
        private readonly List<BookingCallInfo> _calls = new List<BookingCallInfo>();
 
        internal BookingCalls(XElement element)
        {
            var results = element.Elements("Call");
            foreach (var result in results)
            {
                _calls.Add(new BookingCallInfo(result));
            }
        }

        public IEnumerator<BookingCallInfo> GetEnumerator()
        {
            return _calls.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class BookingCallInfo
    {
        internal BookingCallInfo(XElement element)
        {
            Number = element.Element("Number").Value;
            Protocol = element.Element("Protocol").Value;
        }

        public string Number { get; private set; }
        public string Protocol { get; private set; }
    }
}