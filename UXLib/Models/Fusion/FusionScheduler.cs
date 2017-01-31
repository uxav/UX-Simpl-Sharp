using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro; 
using Crestron.SimplSharpPro.Fusion;

// ReSharper disable once CheckNamespace
namespace UXLib.Models.Fusion
{
   public class FusionScheduler : IDisposable
   {
      # region meeting class
      private class Meeting
       {
           public DateTime LastUpdateTime;
           public MeetingDetails MeetingDetails;
       }
      # endregion

      # region adhoc meeting class
      public class AdhocMeeting
       {
          public string Organizer;
          public int StartSlot;
          public int EndSlot;
       }
      # endregion

      # region declarations
      public readonly FusionRoom fusionRoom;
      public bool IsRegistered;
      public bool IsOnline;

      private DateTime _lastLocalTimeThrown;
      private readonly Dictionary <string, CTimer> _meetingTimers;
      private readonly Dictionary<string, Meeting> _meetings = new Dictionary<string,Meeting>();
      private DateTime _lastRequestEndTime;
      private string _lastReceivedXML;

      private readonly string _regex;
      
      public delegate void OnlineStatusChangeDelegate(bool onlineStatus);
      public event OnlineStatusChangeDelegate OnlineStatusChange;
      public delegate void LocalDateTimeUpdateDelegate(DateTime updatedDateTime);
      public event LocalDateTimeUpdateDelegate LocalDateTimeUpdate;
      public delegate void MeetingStartingUpdateDelegate(MeetingDetails nextMeetingDetails);
      public event MeetingStartingUpdateDelegate MeetingStartingUpdate;
      public delegate void MeetingEndingUpdateDelegate(string meetingID);
      public event MeetingEndingUpdateDelegate MeetingEndingUpdate;
      public enum MeetingUpdateStatus {New, Update, Delete}
      public delegate void MeetingUpdateDelegate(MeetingDetails nextMeetingDetails, MeetingUpdateStatus status);

      public event MeetingUpdateDelegate MeetingUpdate;

      private readonly CTimer _fusionClockTimer;
      private readonly CTimer _fusionScheduleTimer;
      private const int FusionMeetingCheckTimespan = 24; // 24 hour
      private const long FusionClockCheckTime = 86400000; // 24 hours
      private const long FusionScheduleCheckTime =   120000; // 2 minutes

       private int _currentDay;
      # endregion

      #region ctor
      public FusionScheduler (FusionController controller)
      {
         // instantiate room 
          fusionRoom = controller.FusionRoom;

         // enable scheduling extenders
          fusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.Use();
          fusionRoom.ExtenderFusionRoomDataReservedSigs.Use();

         // subscribe to event handlers
         fusionRoom.OnlineStatusChange += OnlineStatusChangeHandler;
         fusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.DeviceExtenderSigChange 
            += SchedulingChangeHandler;
         fusionRoom.ExtenderFusionRoomDataReservedSigs.DeviceExtenderSigChange
            += RoomDataChangeHandler;

         // instantiate dictionaries
         _meetingTimers = new Dictionary <string, CTimer> ();
 
         _lastRequestEndTime = DateTime.MinValue;

         // setup timers
         _fusionClockTimer = new CTimer(CTimerRequestFusionClock, null, Timeout.Infinite, Timeout.Infinite);
         _fusionScheduleTimer = new CTimer(CTimerRequestFusionSchedule, null, Timeout.Infinite, Timeout.Infinite);

         // set lastLocalTimeThrown to avoid null reference error later in comparison
         _lastLocalTimeThrown = DateTime.Now;
          _currentDay = -1;

         // set regex
         _regex = ".*";
      }
      # endregion

      # region event invokers
      private void OnOnlineStatusChange(bool onlineStatus)
      {
         var handler = OnlineStatusChange;
         if (handler != null)
         {
            handler(onlineStatus);
         }
      }

      private void OnLocalDateTimeUpdate(DateTime updatedDateTime)
      {
         var handler = LocalDateTimeUpdate;
         if (handler != null)
         {
            handler(updatedDateTime);
         }
      }

      private void OnMeetingStartUpdate(MeetingDetails newMeetingDetails)
      {
         var handler = MeetingStartingUpdate;
         if (handler != null)
         {
            handler(newMeetingDetails);
         }
      }

      private void OnMeetingEndingUpdate(string meetingID)
      {
         var handler = MeetingEndingUpdate;
         if (handler != null)
         {
            handler(meetingID);
         }
      }
      # endregion

      # region event handlers
      private void OnlineStatusChangeHandler(GenericBase currentdevice, OnlineOfflineEventArgs args)
      {
         if (IsOnline == args.DeviceOnLine) return;

         // update property and throw event
         IsOnline = args.DeviceOnLine;
         OnOnlineStatusChange(IsOnline);

         // if room has come online, request room clock and schedule from fusion
         if (IsOnline)
         {
            RequestFusionClock();
            RequestRoomSchedule();

            // request fusion clock in 24 hours
            _fusionClockTimer.Reset(FusionClockCheckTime, Timeout.Infinite);

            // request fusion schedule in 2 minutes
            _fusionScheduleTimer.Reset(FusionScheduleCheckTime, Timeout.Infinite);
         }
         else
         {
            // stop timers if gone offline
            _fusionClockTimer.Stop();
            _fusionScheduleTimer.Stop();
         }
      }

      private void SchedulingChangeHandler(DeviceExtender currentdeviceextender, SigEventArgs args)
      {
         // xml string has changed indicating new data
         if (args.Event == eSigEvent.StringChange)
         {
            //  ScheduleResponse
            if (args.Sig ==
               fusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.ScheduleResponse)
            {
               ProcessFusionSchedule();
            }
            // Adhoc Meeting Response
            else if (args.Sig == fusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.CreateResponse)
            {
               

               // request fusion schedule
               RequestRoomSchedule();
            }
         }
      }

      private void RoomDataChangeHandler(DeviceExtender currentdeviceextender, SigEventArgs args)
      {
         // xml string has changed indicating new data
         if (args.Event == eSigEvent.StringChange)
         {
            // only interested in LocalDateTimeResponse
            if (args.Sig ==
               fusionRoom.ExtenderFusionRoomDataReservedSigs.LocalDateTimeQueryResponse)
            {
               ProcessFusionClock();
            }
         }
      }
      # endregion

      # region CTimer Callbacks
      private void CTimerRequestFusionClock(object obj)
      {
         // request fusion clock
         RequestFusionClock();

         // request fusion clock in 24 hours
         _fusionClockTimer.Reset(FusionClockCheckTime, Timeout.Infinite);
      }

      private void CTimerRequestFusionSchedule(object obj)
      {
         // request fusion schedule
         RequestRoomSchedule();

         // request fusion schedule in 5 minutes
         _fusionScheduleTimer.Reset(FusionScheduleCheckTime, Timeout.Infinite);
      }

      private void CTimerMeetingStart(object obj)
      {
         string meetingID = (string) obj;

         // throw event for meeting start
         OnMeetingStartUpdate(_meetings[meetingID].MeetingDetails);

         // overwrite timer to end timer
         var difference = _meetings[meetingID].MeetingDetails.DateTimeEnd - DateTime.Now;

         _meetingTimers[meetingID].Dispose();
         _meetingTimers[meetingID] = new CTimer(CTimerMeetingEnd, meetingID, difference.Minutes * 60000, Timeout.Infinite);
      }

      private void CTimerMeetingEnd(object obj)
      {
         
         string meetingID = (string)obj;

         // throw event
         OnMeetingEndingUpdate(meetingID);

         // remove from _meetingTimers and _meetings
         _meetingTimers[meetingID].Dispose();
         _meetingTimers.Remove (meetingID);
         _meetings.Remove (meetingID);
      }
      # endregion

      # region fusion xml related
      private void RequestFusionClock()
      {
         // serialize xml and send to fusion
         var clockRequestXml =
            XmlSerialization.Serialize (new LocalTimeRequest {RequestID = "ClockRequest"});

         fusionRoom.ExtenderFusionRoomDataReservedSigs.LocalDateTimeQuery.StringValue =
            clockRequestXml;
      }

       private void RequestRoomSchedule()
       {
           // set up request with correct request info
           var fieldList = new List<string>
           {
               "dtStart",
               "dtEnd",
               "MeetingID",
               "Organizer",
               "Subject",
               "Attendees",
               "Body"
           };

           DateTime startTime = DateTime.Now;
           var scheduleTime = new DateTime (startTime.Year, startTime.Month, startTime.Day, 0, 0, 0);
           var hourSpan = FusionMeetingCheckTimespan;

           if (startTime.Day != _currentDay)
           {
               _currentDay = startTime.Day;
               startTime = new DateTime(startTime.Year, startTime.Month, startTime.Day, 0, 0, 0);
               hourSpan = 24;
               RemoveYesterdaysMeeting(startTime);
           }

           var scheduleRequest = new RequestSchedule
           {
               RequestID = "RoomScheduleRequest",
               //RoomID = _fusionRoom.ParameterRoomName,
               //Start = string.Format("{0:yyyy-MM-ddTHH:mm:ss}", startTime),
               Start = string.Format("{0:yyyy-MM-ddTHH:mm:ss}", scheduleTime),
               HourSpan = hourSpan.ToString("D"),
               FieldList = fieldList
           };
           _lastRequestEndTime = startTime.AddHours(hourSpan);   

           // serialise and send to fusion
           var scheduleRequestXml = XmlSerialization.Serialize(scheduleRequest);

           fusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.ScheduleQuery.StringValue = scheduleRequestXml;
       }

       private void ProcessFusionClock()
      {
         // deserialize and throw event
         var clockResponse = new LocalTimeResponse();
         XmlSerialization.DeSerialize(clockResponse, fusionRoom.ExtenderFusionRoomDataReservedSigs.LocalDateTimeQueryResponse.StringValue);

         // if we havent thrown this time before, store time and throw
         if (clockResponse.LocalDateTime != _lastLocalTimeThrown)
         {
            _lastLocalTimeThrown = clockResponse.LocalDateTime;
            OnLocalDateTimeUpdate (clockResponse.LocalDateTime);
         }
      }

       private void ProcessFusionSchedule()
       {
          // check not a duplicate xml request
          var receivedXml =
             fusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.ScheduleResponse.StringValue;

          if (_lastReceivedXML != null)
          {
             if (_lastReceivedXML == receivedXml) return;
          }

          // assign received xml to last received
          _lastReceivedXML = receivedXml;

          // deserialize xml
          var scheduleResponse = new ScheduleResponse();
          XmlSerialization.DeSerialize(scheduleResponse,
              receivedXml);

           // if RequestID is RVRequest, the remote schedule has changed and we need to re-request
          if (scheduleResponse.RequestID == "RVRequest")
          {
             RequestRoomSchedule();
             return;
          }

          // continue to process
          var tempMeetingDict = new Dictionary<string, Meeting>();
          var updateTime = DateTime.Now;  
          foreach (var fusionmeeting in scheduleResponse.Event)
           {
               Meeting meeting;
               _meetings.TryGetValue(fusionmeeting.MeetingID, out meeting);

               // new meeting
               if (meeting == null)
               {
                   var meetingDetails = CreateMeetingDetails(scheduleResponse, fusionmeeting);

                   // add to meetings
                   _meetings.Add(meetingDetails.MeetingID, new Meeting
                   {
                       MeetingDetails = meetingDetails,
                       LastUpdateTime = updateTime
                       
                   });
                   ReportMeetingUpdate(meetingDetails, MeetingUpdateStatus.New);
                   continue;
               }

               var fusiondateTimeStart = Convert.ToDateTime(fusionmeeting.dtStart);
               var fusiondateTimeEnd = Convert.ToDateTime(fusionmeeting.dtEnd);

               // change to meeting
               if (   (meeting.MeetingDetails.DateTimeStart != fusiondateTimeStart)
                   || (meeting.MeetingDetails.DateTimeEnd != fusiondateTimeEnd)

                   )
               {
                   var meetingDetails = CreateMeetingDetails(scheduleResponse, fusionmeeting);
                   // add to temp meetings
                   tempMeetingDict.Add(meetingDetails.MeetingID, new Meeting
                   {
                      MeetingDetails = meetingDetails,
                      LastUpdateTime = updateTime
                   });

                   ReportMeetingUpdate(meetingDetails, MeetingUpdateStatus.Update);
                   continue;
               }
               _meetings[meeting.MeetingDetails.MeetingID].LastUpdateTime = updateTime;
           }
           
           // copy across tempMeetingDict to _meetings
          foreach (KeyValuePair <string, Meeting> pair in tempMeetingDict)
          {
             if (_meetings.ContainsKey (pair.Key))
             {
                _meetings[pair.Key] = pair.Value;
             }
          }

          RemoveDeletedMeetings(updateTime, _lastRequestEndTime);
       }

       private void RemoveDeletedMeetings(DateTime updateTime, DateTime lastRequestEndTime)
       {
           // Signal the meeting was removed if it was not updated when we last checked
           foreach (var meeting in _meetings.ToList())
           {
               //if (meeting.Value.MeetingDetails.DateTimeEnd < updateTime) continue;
               if (meeting.Value.MeetingDetails.DateTimeStart > lastRequestEndTime) continue;
               if (meeting.Value.LastUpdateTime == updateTime) continue;

               ReportMeetingUpdate(meeting.Value.MeetingDetails, MeetingUpdateStatus.Delete);
               _meetings.Remove(meeting.Key);
           }
       }

       private void RemoveYesterdaysMeeting(DateTime currentday)
       {
           foreach (var meeting in _meetings.Values.ToList())
           {
               if (meeting.MeetingDetails.DateTimeEnd < currentday)
               {
                   _meetings.Remove(meeting.MeetingDetails.MeetingID);
                   ReportMeetingUpdate(meeting.MeetingDetails, MeetingUpdateStatus.Delete);
               }
           }
       }
       private void ReportMeetingUpdate(MeetingDetails meetingDetail, MeetingUpdateStatus status)
       {
           if (MeetingUpdate == null) return;
           MeetingUpdate(meetingDetail, status);
       }
      # endregion

      # region create meeting
       public void CreateMeeting(string organizer, int timeSpan)
       {
          try
          {
             var adhocMeeting = new CreateSchedule()
             {
                RequestID = "Plugin Panel Adhoc Meeting",
                Event =
                   new Event()
                   {
                      dtStart = string.Format("{0:yyyy-MM-ddTHH:mm:ss}", DateTime.Now),
                      dtEnd = string.Format("{0:yyyy-MM-ddTHH:mm:ss}", DateTime.Now.AddMinutes(timeSpan)),
                      Organizer = organizer,
                      Subject = "Adhoc Meeting"
                   }
             };

             var xmlstring = XmlSerialization.Serialize(adhocMeeting);

             fusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.CreateMeeting.StringValue =
                xmlstring;
          }
          catch (Exception ex)
          {

          }

       }
       #endregion

      # region update sigs
      // serial
      public bool UpdateSig(uint joinNumber, string newValue)
      {
         if (fusionRoom == null) return (false);

         if (!fusionRoom.UserDefinedStringSigDetails.Contains(joinNumber))
         {
            return (false);
         }

         try
         {
            fusionRoom.UserDefinedStringSigDetails[joinNumber].InputSig.StringValue = newValue;
            return (true);
         }
         catch (Exception)
         {
            return (false);
         }
      }

      // digital
      public bool UpdateSig(uint joinNumber, bool newValue)
      {
         if (fusionRoom == null) return (false);

         if (!fusionRoom.UserDefinedBooleanSigDetails.Contains(joinNumber))
         {
            return (false);
         }

         try
         {
            fusionRoom.UserDefinedBooleanSigDetails[joinNumber].InputSig.BoolValue = newValue;
            return (true);
         }
         catch (Exception)
         {
            return (false);
         }
      }

      // analog
      public bool UpdateSig(uint joinNumber, ushort newValue)
      {
         if (fusionRoom == null) return (false);

         if (!fusionRoom.UserDefinedUShortSigDetails.Contains(joinNumber))
         {
            return (false);
         }

         try
         {
            fusionRoom.UserDefinedUShortSigDetails[joinNumber].InputSig.UShortValue = newValue;
            return (true);
         }
         catch (Exception)
         {
            return (false);
         }
      }
      # endregion

      # region MeetingDetails related
      private MeetingDetails CreateMeetingDetails(ScheduleResponse scheduleResponse, Event meeting)
      {
         var meetingDetails = new MeetingDetails
         {
             MeetingID = meeting.MeetingID,
             RoomName = scheduleResponse.RoomName,
             Subject = meeting.Subject,
             DateTimeStart = Convert.ToDateTime(meeting.dtStart),
             DateTimeEnd = Convert.ToDateTime(meeting.dtEnd),
             Organizer = meeting.Organizer,
             RequiredAttendees = meeting.Attendees.Required.Attendee,
             OptionalAttendees = meeting.Attendees.Optional.Attendee,
             RegexMatched = RunRegex(meeting.Body)
         };

         return (meetingDetails);
      }

      private string RunRegex(string toLookAt)
      {
         var returnString = String.Empty;
         var regex = new Regex (_regex);

         foreach (Match match in regex.Matches(toLookAt))
         {
            returnString = match.Groups[0].Value;
            break;
         }

         return (returnString);
      }

      public void DumpMeetingDetails(MeetingDetails paramMeetingDetails)
      {
         CrestronConsole.PrintLine("***DumpMeetingDetails START***");
         CrestronConsole.PrintLine("MeetingID: {0}", paramMeetingDetails.MeetingID);
         CrestronConsole.PrintLine("RoomName: {0}", paramMeetingDetails.RoomName);
         CrestronConsole.PrintLine("Subject: {0}", paramMeetingDetails.Subject);
         CrestronConsole.PrintLine("DateTimeStart: {0}", paramMeetingDetails.DateTimeStart.ToString());
         CrestronConsole.PrintLine("DateTimeEnd: {0}", paramMeetingDetails.DateTimeEnd.ToString());
         CrestronConsole.PrintLine("Organizer: {0}", paramMeetingDetails.Organizer);

         // required attendees
         if (paramMeetingDetails.RequiredAttendees != null)
         {
            foreach (var attendee in paramMeetingDetails.RequiredAttendees)
            {
               CrestronConsole.PrintLine("Required Attendee: {0}", attendee);
            }
         }

         // optional attendees
         if (paramMeetingDetails.OptionalAttendees != null)
         {
            foreach (var attendee in paramMeetingDetails.OptionalAttendees)
            {
               CrestronConsole.PrintLine("Required Attendee: {0}", attendee);
            }
         }

         CrestronConsole.PrintLine("Matched Regex: {0}", paramMeetingDetails.RegexMatched);
         CrestronConsole.PrintLine("***DumpMeetingDetails END***");   
      }
      # endregion

      # region time cleanup
      public void CleanUptimers()
       {
           _fusionClockTimer.Dispose();
           _fusionScheduleTimer.Dispose();
           if (_meetingTimers == null) return;

           foreach (var timer in _meetingTimers.Values)
           {
               if (timer != null)
                   timer.Dispose();
           }
       }
      # endregion

      # region IDisposable
      private bool _disposed;
       public void Dispose(bool paramDisposing)
       {
           if (!_disposed)
           {
               if (paramDisposing)
               {
                   CleanUptimers();
               }

               _disposed = true;
           }
       }

       public void Dispose()
       {
          Dispose(true);
       }

      # endregion

      # region UnRegister
       public eDeviceRegistrationUnRegistrationResponse UnRegister()
       {
           CleanUptimers();
           return fusionRoom.UnRegister();
       }
       # endregion
   }
}

