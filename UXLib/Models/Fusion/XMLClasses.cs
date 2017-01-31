using System;
using System.Collections.Generic;
using System.Globalization;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronXml;
using Crestron.SimplSharp.CrestronXmlLinq;

namespace UXLib.Models.Fusion
{
   # region LocalTimeRequest
   public class LocalTimeRequest : IXmlSerializable
   {
      public string RequestID { get; set; }

      public void Serialize(XmlWriter xml)
      {
         xml.WriteStartElement(GetType().Name);
         xml.WriteElementString("RequestID", RequestID);
         xml.WriteEndElement();
      }

      public void Deserialize(XElement node)
      {
         XElement field;

         field = node.Element("RequestID");
         if (field != null)
            RequestID = field.Value;
      }
   }
   # endregion

   # region LocalTimeResponse
   public class LocalTimeResponse : IXmlSerializable
   {
      public string RequestID { get; set; }
      public DateTime LocalDateTime { get; set; }

      public void Serialize(XmlWriter xml)
      {
         xml.WriteStartElement(GetType().Name);
         xml.WriteElementString("RequestID", RequestID);
         xml.WriteElementString("LocalDateTime", LocalDateTime.ToString(CultureInfo.InvariantCulture));
         xml.WriteEndElement();
      }

      public void Deserialize(XElement node)
      {
         XElement field;

         field = node.Element("RequestID");
         if (field != null)
            RequestID = field.Value;

         // if there is an issue write epoch later
         var issue = false;
         field = node.Element ("LocalDateTime");
         if (field != null)
         {
            try
            {
               LocalDateTime = Convert.ToDateTime(field.Value);
            }
            catch (Exception)
            {
               issue = true;
               ErrorLog.Error("FusionSchedule::XmlSerialization: Could not cast {0}", field.Value);
            }
         }
         else
         {
            issue = true;
         }

         if (issue)
         {
            LocalDateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            ErrorLog.Error("Writing LocalDateTime to Epoch for RequestID: {0}", RequestID);
         }
      }
   }
   # endregion

   # region RequestSchedule
   public class RequestSchedule : IXmlSerializable
   {
      public string RequestID { get; set; }
      public string RoomID { get; set; }
      public string Start { get; set; }
      public string HourSpan { get; set; }
      public List <string> FieldList { get; set; }

      public void Serialize(XmlWriter xml)
      {
         xml.WriteStartElement(GetType().Name);
         xml.WriteElementString("RequestID", RequestID);
         xml.WriteElementString("RoomID", RoomID);
         xml.WriteElementString("Start", Start);
         xml.WriteElementString("HourSpan", HourSpan);

         if (FieldList != null)
         {
            xml.WriteStartElement("FieldList");
            foreach (var field in FieldList)
            {
               xml.WriteElementString ("Field", field);
            }
            xml.WriteEndElement();
         }

         xml.WriteEndElement();
      }

      public void Deserialize(XElement node)
      {
         XElement field;

         field = node.Element("RequestID");
         if (field != null)
            RequestID = field.Value;

         field = node.Element("RoomID");
         if (field != null)
            RoomID = field.Value;

         field = node.Element("Start");
         if (field != null)
            Start = field.Value;

         field = node.Element("HourSpan");
         if (field != null)
            HourSpan = field.Value;

         field = node.Element("FieldList");
         if (field != null)
         {
            FieldList = new List<string>();

            var xfields = field.Elements("Field");
            if (xfields != null)
            {
               foreach (var xfield in xfields)
               {
                  FieldList.Add(xfield.Value);
               }
            }
         }
      }
   }
   # endregion

   # region ScheduleResponse
   public class Optional : IXmlSerializable
   {
      public List<string> Attendee { get; set; }

      public void Serialize(XmlWriter xml)
      {
         xml.WriteStartElement(GetType().Name);

         if (Attendee != null)
         {
            foreach (var xattendee in Attendee)
            {
               xml.WriteElementString("Attendee", xattendee);
            }
         }

         xml.WriteEndElement();
      }

      public void Deserialize(XElement node)
      {
         Attendee = new List<string>();

         var xfields = node.Elements("Attendee");
         foreach (var xfield in xfields)
         {
            Attendee.Add(xfield.Value);
         }
      }
   }

	public class Required : IXmlSerializable
   {
		public List<string> Attendee { get; set; }

      public void Serialize(XmlWriter xml)
      {
         xml.WriteStartElement(GetType().Name);

         if (Attendee != null)
         {
            foreach (var xattendee in Attendee)
            {
               xml.WriteElementString("Attendee", xattendee);
            }
         }

         xml.WriteEndElement();
      }

      public void Deserialize(XElement node)
      {
         Attendee = new List<string>();

         var xfields = node.Elements("Attendee");
         foreach (var xfield in xfields)
         {
            Attendee.Add(xfield.Value);
         }
      }
	}

	public class Attendees : IXmlSerializable
   {
		public Required Required { get; set; }
		public Optional Optional { get; set; }

	   public void Serialize(XmlWriter xml)
	   {
         xml.WriteStartElement(GetType().Name);

	      if (Required != null)
	      {
	         Required.Serialize(xml);
	      }
	      else
	      {
	         xml.WriteElementString("Required", "");
	      }

         if (Optional != null)
         {
            Optional.Serialize(xml);
         }
         else
         {
            xml.WriteElementString("Optional", "");
         }
	   }

	   public void Deserialize(XElement node)
	   {
	      XElement field;

	      field = node.Element ("Required");
	      if (field != null)
	      {
	         Required = new Required();
            Required.Deserialize(field);
	      }
	      else
	      {
	         // create empty class
            field = new XElement("");
            Required = new Required();
            Required.Deserialize(field);
	      }

         field = node.Element("Optional");
         if (field != null)
         {
            Optional = new Optional();
            Optional.Deserialize(field);
         }
         else
         {
            // create empty class
            field = new XElement("");
            Optional = new Optional();
            Optional.Deserialize(field);
         }
	   }
   }

	public class Room : IXmlSerializable
   {
		public string ID { get; set; }
		public string Name { get; set; }
		public string MPType { get; set; }

	   public void Serialize(XmlWriter xml)
	   {
         xml.WriteStartElement(GetType().Name);

         xml.WriteElementString("ID", ID);
         xml.WriteElementString("Name", Name);
         xml.WriteElementString("MPType", MPType);

         xml.WriteEndElement();
	   }

	   public void Deserialize(XElement node)
	   {
	      XElement field;

	      field = node.Element("ID");
	      if (field != null)
	         ID = field.Value;

         field = node.Element("Name");
         if (field != null)
            Name = field.Value;

         field = node.Element("MPType");
         if (field != null)
            MPType = field.Value;
	   }
   }

   public class Resources : IXmlSerializable
   {
      public List<Room> Rooms { get; set; }

      public void Serialize(XmlWriter xml)
      {
         xml.WriteStartElement(GetType().Name);

         if (Rooms != null)
         {
            xml.WriteStartElement("Rooms");
            foreach (var room in Rooms)
            {
               room.Serialize(xml);
            }
            xml.WriteEndElement();
         }

         xml.WriteEndElement();
      }

      public void Deserialize(XElement node)
      {
         XElement field;

         field = node.Element("Rooms");
         if (field != null)
         {
            var xrooms = field.Elements("Room");
            if (xrooms != null)
            {
               Rooms = new List<Room>();
               foreach (var xroom in xrooms)
               {
                  var room = new Room();
                  room.Deserialize(xroom);
                  Rooms.Add(room);
               }
            }
         }
      }
   }

	public class LiveMeeting : IXmlSerializable
   {
		public string URL { get; set; }
		public string ID { get; set; }
		public string Key { get; set; }

      public void Serialize(XmlWriter xml)
      {
         xml.WriteStartElement(GetType().Name);

         xml.WriteElementString("URL", URL);
         xml.WriteElementString("ID", ID);
         xml.WriteElementString("Key", Key);

         xml.WriteEndElement();
      }

      public void Deserialize(XElement node)
      {
         XElement field;

         field = node.Element("URL");
         if (field != null)
            URL = field.Value;

         field = node.Element("ID");
         if (field != null)
            ID = field.Value;

         field = node.Element("Key");
         if (field != null)
            Key = field.Value;
      }
   }

	public class LiveMeetingURL : IXmlSerializable
   {
		public LiveMeeting LiveMeeting { get; set; }

	   public void Serialize(XmlWriter xml)
	   {
         xml.WriteStartElement(GetType().Name);

         if (LiveMeeting != null)
	         LiveMeeting.Serialize (xml);
	   }

	   public void Deserialize(XElement node)
	   {
	      XElement field;

         LiveMeeting = new LiveMeeting();
	      field = node.Element("LiveMeeting");
	      if (field != null)
	      {
	         LiveMeeting.Deserialize(field);
	      }
	   }
   }

	public class Event : IXmlSerializable
   {
		public string MeetingID { get; set; }
		public string RVMeetingID { get; set; }
		public string Recurring { get; set; }
		public string InstanceID { get; set; }
		public string dtStart { get; set; }
		public string dtEnd { get; set; }
		public string Organizer { get; set; }
		public Attendees Attendees { get; set; }
		public Resources Resources { get; set; }
		public string IsEvent { get; set; }
		public string IsRoomViewMeeting { get; set; }
		public string IsPrivate { get; set; }
		public string IsExchangePrivate { get; set; }
		public string MeetingTypes { get; set; }
		public string ParticipantCode { get; set; }
		public string PhoneNo { get; set; }
		public string WelcomeMsg { get; set; }
		public string Subject { get; set; }
		public LiveMeetingURL LiveMeetingURL { get; set; }
		public string ShareDocPath { get; set; }
		public string Location { get; set; }
        public string HaveResources { get; set; }
        public string HaveAttendees { get; set; }
        public string Body { get; set; }

	    public void Serialize(XmlWriter xml)
	    {
	        xml.WriteStartElement(GetType().Name);

	        xml.WriteElementString("MeetingID", MeetingID);
	        xml.WriteElementString("RVMeetingID", RVMeetingID);
	        xml.WriteElementString("Recurring", Recurring);
	        xml.WriteElementString("InstanceID", InstanceID);
	        xml.WriteElementString("dtStart", dtStart);
	        xml.WriteElementString("dtEnd", dtEnd);
	        xml.WriteElementString("Organizer", Organizer);

	        if (Attendees != null)
	            Attendees.Serialize(xml);

	        if (Resources != null)
	            Resources.Serialize(xml);

	        xml.WriteElementString("IsEvent", IsEvent);
	        xml.WriteElementString("IsRoomViewMeeting", IsRoomViewMeeting);
	        xml.WriteElementString("IsPrivate", IsPrivate);
	        xml.WriteElementString("IsExchangePrivate", IsExchangePrivate);
	        xml.WriteElementString("MeetingTypes", MeetingTypes);
	        xml.WriteElementString("ParticipantCode", ParticipantCode);
	        xml.WriteElementString("PhoneNo", PhoneNo);
	        xml.WriteElementString("WelcomeMsg", WelcomeMsg);
	        xml.WriteElementString("Subject", Subject);

	        if (LiveMeetingURL != null)
	            LiveMeetingURL.Serialize(xml);

	        xml.WriteElementString("ShareDocPath", ShareDocPath);
	        xml.WriteElementString("Location", Location);

	        if (HaveResources != null)
	            xml.WriteElementString("HaveResources", HaveResources);

	        if (HaveAttendees != null)
	            xml.WriteElementString("HaveAttendees", HaveAttendees);

	        if (Body != null)
	            xml.WriteElementString("Body", Body);

	        xml.WriteEndElement();

	    }

	    public void Deserialize(XElement node)
	   {
	      XElement field;

         field = node.Element("MeetingID");
         if (field != null)
            MeetingID = field.Value;

         field = node.Element("RVMeetingID");
         if (field != null)
            RVMeetingID = field.Value;

         field = node.Element("Recurring");
         if (field != null)
            Recurring = field.Value;

         field = node.Element("InstanceID");
         if (field != null)
            InstanceID = field.Value;

         field = node.Element("dtStart");
         if (field != null)
            dtStart = field.Value;

         field = node.Element("dtEnd");
         if (field != null)
            dtEnd = field.Value;

         field = node.Element("Organizer");
         if (field != null)
            Organizer = field.Value;

         Attendees = new Attendees();
	      field = node.Element ("Attendees");
	      if (field != null)
	      {
	         Attendees.Deserialize (field);
	      }

         Resources = new Resources();
	      field = node.Element ("Resources");
	      if (field != null)
	      {
	         Resources = new Resources ();
	         Resources.Deserialize (field);
	      }
         
         field = node.Element("IsEvent");
         if (field != null)
            IsEvent = field.Value;

         field = node.Element("IsRoomViewMeeting");
         if (field != null)
            IsRoomViewMeeting = field.Value;

         field = node.Element("IsPrivate");
         if (field != null)
            IsPrivate = field.Value;

         field = node.Element("IsExchangePrivate");
         if (field != null)
            IsExchangePrivate = field.Value;

         field = node.Element("MeetingTypes");
         if (field != null)
            MeetingTypes = field.Value;

         field = node.Element("ParticipantCode");
         if (field != null)
            ParticipantCode = field.Value;

         field = node.Element("PhoneNo");
         if (field != null)
            PhoneNo = field.Value;

         field = node.Element("WelcomeMsg");
         if (field != null)
            WelcomeMsg = field.Value;

         field = node.Element("Subject");
         if (field != null)
            Subject = field.Value;

         LiveMeetingURL = new LiveMeetingURL();
         field = node.Element("LiveMeetingURL");
         if (field != null)
         {
            LiveMeetingURL.Deserialize(field);
         }
         
         field = node.Element("ShareDocPath");
         if (field != null)
            ShareDocPath = field.Value;

         field = node.Element("Location");
         if (field != null)
            Location = field.Value;

         field = node.Element("HaveResources");
         if (field != null)
            HaveResources = field.Value;

         field = node.Element("HaveAttendees");
         if (field != null)
            HaveAttendees = field.Value;

         field = node.Element("Body");
         if (field != null)
            Body = field.Value;
	   }
   }

   public class ScheduleResponse : IXmlSerializable
   {
		public string RequestID { get; set; }
		public string RoomID { get; set; }
		public string RoomName { get; set; }
		public List<Event> Event { get; set; }

      public void Serialize(XmlWriter xml)
      {
         xml.WriteStartElement(GetType().Name);
         xml.WriteElementString("RequestID", RequestID);
         xml.WriteElementString("RoomID", RoomID);
         xml.WriteElementString("RoomName", RoomName);

         if (Event != null)
         {
            foreach (var xevent in Event)
            {
               xevent.Serialize(xml);
            }
            xml.WriteEndElement();
         }
         else
         {
            xml.WriteElementString("Event", "");
         }

         xml.WriteEndElement();
      }

      public void Deserialize(XElement node)
      {
         XElement field;

         field = node.Element("RequestID");
         if (field != null)
            RequestID = field.Value;

         field = node.Element("RoomID");
         if (field != null)
            RoomID = field.Value;

         field = node.Element("RoomName");
         if (field != null)
            RoomName = field.Value;

         Event = new List <Event>();
         var fields = node.Elements("Event");
         if (fields != null)
         {
            foreach (var xfield in fields)
            {
               var singleEvent = new Event ();
               singleEvent.Deserialize (xfield);
               Event.Add (singleEvent);
            }
         }
      }
   }
   # endregion

   # region CreateSchedule
   public class CreateSchedule : IXmlSerializable
   {
      public string RequestID { get; set; }
      public Event Event { get; set; }

      public void Serialize(XmlWriter xml)
      {
         xml.WriteStartElement(GetType().Name);
         xml.WriteElementString("RequestID", RequestID);

         if (Event != null)
         {
            Event.Serialize(xml);
         }
         else
         {
            xml.WriteElementString("Event", "");
         }

         xml.WriteEndElement();
      }

      public void Deserialize(XElement node)
      {
         XElement field;

         field = node.Element("RequestID");
         if (field != null)
            RequestID = field.Value;

         field = node.Element("Event");
         var singleEvent = new Event();
         singleEvent.Deserialize(field);

         Event = singleEvent;
      }
   }
   # endregion
}
