using System;
using System.Collections.Generic;

namespace UXLib.Models.Fusion
{
   public class MeetingDetails
   {
      public string MeetingID { get; set; }
      public string RoomName { get; set; }
      public string Subject { get; set; }
      public DateTime DateTimeStart { get; set; }
      public DateTime DateTimeEnd { get; set; }
      public string Organizer { get; set; }
      public List<string> RequiredAttendees { get; set; }
      public List<string> OptionalAttendees { get; set; }
      public string RegexMatched { get; set; }
   }
}