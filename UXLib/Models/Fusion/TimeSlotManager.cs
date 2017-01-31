// Copyright (C) 2015 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
using System;
using System.Collections.Generic;
using System.Linq;
using Crestron.SimplSharp.Reflection;
using Crestron.SimplSharpFramework.Panel;

namespace RGAPlugin.WinCe.Fusion
{
   public class TimeSlotManager
   {
      # region properties
      private readonly uint _earliestTimeSlot;
      private readonly uint _latestTimeSlot;
      private readonly uint _totalTimeSlots;
      private readonly IPanelManager _PM;
      private readonly string PanelId;
      private readonly string CalendarList;
      private Dictionary<uint, Dictionary<uint, RoomScheduleController.Meeting>> _timeSlotMapper;
      # endregion

      # region ctor
      public TimeSlotManager(uint earliestTimeSlot, uint latestTimeSlot, string panelId, string calendarList, IPanelManager PM)
      {
         _earliestTimeSlot = earliestTimeSlot;
         _latestTimeSlot = latestTimeSlot;
         PanelId = panelId;
         CalendarList = calendarList;
         _PM = PM;
         _timeSlotMapper = new Dictionary<uint, Dictionary<uint, RoomScheduleController.Meeting>>();


         // determine total timeslots
         if (_earliestTimeSlot > _latestTimeSlot)
         {
            _totalTimeSlots = 0;
         }
         else
         {
            _totalTimeSlots = (_latestTimeSlot - _earliestTimeSlot) * 2;
         }

         // populate _timeSlotMapper
         for (uint i = 0; i <= _totalTimeSlots; i++)
         {
            var nested = new Dictionary<uint, RoomScheduleController.Meeting> { { 1, new RoomScheduleController.Meeting() }, { 2, new RoomScheduleController.Meeting() } };
            _timeSlotMapper.Add(i, nested);
         }
      }
      # endregion

      # region add/remove methods
      public void AddMeeting(RoomScheduleController.Meeting meetingDetails)
      {
         var startSlot = CalcTimeSlot(meetingDetails.StartTime, true);
         var endSlot = CalcTimeSlot(meetingDetails.EndTime, false);
         var slotDiff = CalcSlotDiff (startSlot, endSlot);

         var firstIteration = true;
         while (slotDiff > 0)
         {
            // if invalid slot time then return
            if (!TimeIsValid(startSlot)) return;

            // if timeslot is free
            if (TimeSlotIsFree(startSlot))
            {
               // add to _timeSlotMapper and UI
               _timeSlotMapper[startSlot.HourSlot][startSlot.MinSlot] = new RoomScheduleController.Meeting();
               _timeSlotMapper[startSlot.HourSlot][startSlot.MinSlot] =  meetingDetails;

               if (firstIteration)
               {
                  var slotText = startSlot.MinSlot == 1 ? "TopSlotOrganizer" : "BottomSlotOrganizer";
                  _PM.SmartObjectText (PanelId, CalendarList, startSlot.HourSlot, slotText,
                                       meetingDetails.Organizer);

                  firstIteration = false;
               }

               DrawSlot(startSlot.HourSlot, startSlot.MinSlot, CalendarEntry.InMeeting);
            }

            // increment tempSlot
            if (startSlot.MinSlot == 2)
            {
               startSlot.HourSlot++;
               startSlot.MinSlot = 1;
            }
            else
            {
               startSlot.MinSlot++;
            }

            slotDiff--;
         }
      }

      public void RemoveMeeting(RoomScheduleController.Meeting meetingDetails)
      {
         if (meetingDetails == null) return;

         var toRemove = new List <CalendarSlot> ();
         var firstIteration = true;

         foreach (var slotId in _timeSlotMapper.Keys.ToList())
         {
            foreach (KeyValuePair<uint, RoomScheduleController.Meeting> subSlotPair in _timeSlotMapper[slotId])
            {
               var hasMeeting = subSlotPair.Value.Id != null;
               if (hasMeeting)
               {
                  if (subSlotPair.Value.Id == meetingDetails.Id)
                  {
                     if (firstIteration)
                     {
                        if (subSlotPair.Key == 1)
                           _PM.SmartObjectText (PanelId, CalendarList, slotId, "TopSlotOrganizer", "");

                        if (subSlotPair.Key == 2)
                           _PM.SmartObjectText(PanelId, CalendarList, slotId, "BottomSlotOrganizer", "");

                        firstIteration = false;
                     }

                     DrawSlot(slotId, subSlotPair.Key, CalendarEntry.OpenTimeSlot);
                     toRemove.Add(new CalendarSlot(slotId, subSlotPair.Key));
                  }
               }
            }
         }

         foreach (var singleSlot in toRemove)
         {
            _timeSlotMapper[singleSlot.HourSlot][singleSlot.MinSlot] = new RoomScheduleController.Meeting();
         }
      }
      # endregion

      # region adhoc preview
      public void ChangeAdhocPreview(int slotChosen)
      {
         // determine slots to update
         var adhocMeeting = new RoomScheduleController.Meeting { Id = "Preview" };
         var currentTimeSlot = CalcTimeSlot(DateTime.Now, true);
         var endTimeslot = CalcTimeSlot (DateTime.Now.AddMinutes (slotChosen*15), false);
         var slotDiff = CalcSlotDiff (currentTimeSlot, endTimeslot);

         // Reset Preview
         ResetPreviews();

         // add preview(s)
         while (slotDiff > 0)
         {
            // if invalid slot time then return
            if (!TimeIsValid(currentTimeSlot)) return;

            // if timeslot is free
            if (TimeSlotIsFree(currentTimeSlot))
            {
               // add to _timeSlotMapper and UI
               _timeSlotMapper[currentTimeSlot.HourSlot][currentTimeSlot.MinSlot] = new RoomScheduleController.Meeting();
               _timeSlotMapper[currentTimeSlot.HourSlot][currentTimeSlot.MinSlot] = adhocMeeting;

               DrawSlot(currentTimeSlot.HourSlot, currentTimeSlot.MinSlot, CalendarEntry.AdhocPreview);
            }

            // increment tempSlot
            if (currentTimeSlot.MinSlot == 2)
            {
               currentTimeSlot.HourSlot++;
               currentTimeSlot.MinSlot = 1;
            }
            else
            {
               currentTimeSlot.MinSlot++;
            }

            slotDiff--;
         }
      }

      public void ResetPreviews()
      {
         RemoveMeeting(new RoomScheduleController.Meeting { Id = "Preview" });
      }
      # endregion

      # region helper methods
      public void DrawTimeSlots()
      {
         foreach (var slotId in _timeSlotMapper.Keys.ToList())
         {
            foreach (KeyValuePair<uint, RoomScheduleController.Meeting> subSlotPair in _timeSlotMapper[slotId])
            {
               bool bHasMeeting = _timeSlotMapper[slotId][subSlotPair.Key] != null &&
                                  _timeSlotMapper[slotId][subSlotPair.Key].Id != null;

               DrawSlot (slotId, subSlotPair.Key,
                         bHasMeeting ? CalendarEntry.InMeeting : CalendarEntry.OpenTimeSlot);
            }
         }
      }

      public void DrawSlot(uint slotId, uint subSlotId, CalendarEntry entryType)
      {
         switch (entryType)
         {
            case(CalendarEntry.OpenTimeSlot):
               if (subSlotId == 1)
               {
                  _PM.SmartObjectIconAnalogSelect (PanelId, CalendarList, slotId, "TopRed",
                                                   (uint)CalenderEntryCode.OpenTimeSlot);
                  _PM.SmartObjectIconAnalogSelect(PanelId, CalendarList, slotId, "TopGreen",
                                                   (uint)CalenderEntryCode.OpenTimeSlot);
                  _PM.SmartObjectIconAnalogSelect(PanelId, CalendarList, slotId, "TopBlue",
                                                   (uint)CalenderEntryCode.OpenTimeSlot);
               }
               else
               {
                  _PM.SmartObjectIconAnalogSelect(PanelId, CalendarList, slotId, "BottomRed",
                                                   (uint)CalenderEntryCode.OpenTimeSlot);
                  _PM.SmartObjectIconAnalogSelect(PanelId, CalendarList, slotId, "BottomGreen",
                                                   (uint)CalenderEntryCode.OpenTimeSlot);
                  _PM.SmartObjectIconAnalogSelect(PanelId, CalendarList, slotId, "BottomBlue",
                                                   (uint)CalenderEntryCode.OpenTimeSlot);
               }
               break;
            case(CalendarEntry.InMeeting):
               if (subSlotId == 1)
               {
                  _PM.SmartObjectIconAnalogSelect(PanelId, CalendarList, slotId, "TopRed",
                                                   (uint)CalenderEntryCode.InMeetingRed);
                  _PM.SmartObjectIconAnalogSelect(PanelId, CalendarList, slotId, "TopGreen",
                                                   (uint)CalenderEntryCode.InMeetingGreen);
                  _PM.SmartObjectIconAnalogSelect(PanelId, CalendarList, slotId, "TopBlue",
                                                   (uint)CalenderEntryCode.InMeetingBlue);
               }
               else
               {
                  _PM.SmartObjectIconAnalogSelect(PanelId, CalendarList, slotId, "BottomRed",
                                                   (uint)CalenderEntryCode.InMeetingRed);
                  _PM.SmartObjectIconAnalogSelect(PanelId, CalendarList, slotId, "BottomGreen",
                                                   (uint)CalenderEntryCode.InMeetingGreen);
                  _PM.SmartObjectIconAnalogSelect(PanelId, CalendarList, slotId, "BottomBlue",
                                                   (uint)CalenderEntryCode.InMeetingBlue);
               }
               break;
            case(CalendarEntry.AdhocPreview):
               if (subSlotId == 1)
               {
                  _PM.SmartObjectIconAnalogSelect(PanelId, CalendarList, slotId, "TopRed",
                                                   (uint)CalenderEntryCode.AdhocRed);
                  _PM.SmartObjectIconAnalogSelect(PanelId, CalendarList, slotId, "TopGreen",
                                                   (uint)CalenderEntryCode.AdhocGreen);
                  _PM.SmartObjectIconAnalogSelect(PanelId, CalendarList, slotId, "TopBlue",
                                                   (uint)CalenderEntryCode.AdhocBlue);
               }
               else
               {
                  _PM.SmartObjectIconAnalogSelect(PanelId, CalendarList, slotId, "BottomRed",
                                                   (uint)CalenderEntryCode.AdhocRed);
                  _PM.SmartObjectIconAnalogSelect(PanelId, CalendarList, slotId, "BottomGreen",
                                                   (uint)CalenderEntryCode.AdhocGreen);
                  _PM.SmartObjectIconAnalogSelect(PanelId, CalendarList, slotId, "BottomBlue",
                                                   (uint)CalenderEntryCode.AdhocBlue);
               }
               break;
         }
      }

      public CalendarSlot CalcTimeSlot(DateTime time, bool bStartSlot)
      {
         uint hourSlot = 0;
         uint minSlot = 0;

         // only calculate if valid time zone for meetings [start slots]
         if (bStartSlot && time.Hour < _latestTimeSlot && time.Hour >= _earliestTimeSlot)
         {
            // determine slot from hour
            hourSlot = (uint)((time.Hour - _earliestTimeSlot) * 2) + (uint)(time.Minute >= 30 ? 1 : 0);

            // mode 1
            if (time.Minute >= 0 && time.Minute < 15)
            {
               minSlot = 1;
            }
            // mode 2
            else if (time.Minute >= 15 && time.Minute < 30)
            {
               minSlot = 2;
            }
            // mode 3
            else if (time.Minute >= 30 && time.Minute < 45)
            {
               minSlot = 1;
            }
            // mode 4
            else if (time.Minute >= 45 && time.Minute <= 59)
            {
               minSlot = 2;
            }
         }
         // only calculate if valid time zone for meetings [end slots]
         else if (!bStartSlot && time.Hour >= _earliestTimeSlot)
         {
            // determine slot from hour
            hourSlot = (uint)((time.Hour - _earliestTimeSlot) * 2) + (uint)(time.Minute > 30 ? 1 : 0);

            // mode 1
            if (time.Minute > 0 && time.Minute <= 15)
            {
               minSlot = 1;
            }
            // mode 2
            else if (time.Minute > 15 && time.Minute <= 30)
            {
               minSlot = 2;
            }
            // mode 3
            else if (time.Minute > 30 && time.Minute <= 45)
            {
               minSlot = 1;
            }
            // mode 4
            else if (time.Minute > 45 && time.Minute <= 59)
            {
               minSlot = 2;
            }
            // 0 minute finish
            else if (time.Minute == 0)
            {
               hourSlot--;
               minSlot = 2;
            }
         }
         return (new CalendarSlot(hourSlot, minSlot));
      }

      public bool TimeSlotIsFree(CalendarSlot slot)
      {

         if (slot.HourSlot == 0 && slot.MinSlot == 0) return false;
         return (_timeSlotMapper[slot.HourSlot][slot.MinSlot].Id == null);
      }

      public bool TimeIsValid(CalendarSlot slot)
      {
         return (slot.HourSlot <= _totalTimeSlots);
      }

      public uint CalcSlotDiff(CalendarSlot startSlot, CalendarSlot endSlot)
      {
         // startSlot is later than endSlot
         if (startSlot.HourSlot > endSlot.HourSlot)
         {
            return (0);
         }

         // calc slots
         uint slotDiff = 0;
         var tempSlot = new CalendarSlot (startSlot.HourSlot, startSlot.MinSlot);
         var bContinue = true;
         while (bContinue)
         {
            // same time
            if (tempSlot.HourSlot == endSlot.HourSlot && tempSlot.MinSlot == endSlot.MinSlot)
            {
               slotDiff++;
               bContinue = false;
            }
            else
            {
               // increment tempSlot
               if (tempSlot.MinSlot == 2)
               {
                  tempSlot.HourSlot++;
                  tempSlot.MinSlot = 1;
               }
               else
               {
                  tempSlot.MinSlot++;
               }

               slotDiff++;
            }
         }
         return (slotDiff);
      }
      # endregion
   }

   # region CalendarSlot Class
   public class CalendarSlot
   {
      public uint HourSlot;
      public uint MinSlot;

      public CalendarSlot(uint pHourSlot, uint pMinSlot)
      {
         HourSlot = pHourSlot;
         MinSlot = pMinSlot;
      }
   }
   # endregion

   # region enums
   public enum CalendarEntry
   {
      OpenTimeSlot = 1,
      InMeeting,
      AdhocPreview
   }

   public enum CalenderEntryCode
   {
      OpenTimeSlot = 247,
      InMeetingRed = 123,
      InMeetingGreen = 124,
      InMeetingBlue = 125,
      AdhocRed = 198,
      AdhocGreen = 199,
      AdhocBlue = 200
   }
   # endregion
}