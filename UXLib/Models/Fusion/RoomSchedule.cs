// Copyright (C) 2015 to the present, Crestron Electronics, Inc.
// All rights reserved.
// No part of this software may be reproduced in any form, machine
// or natural, without the express written consent of Crestron Electronics.
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Crestron.Fusion;
using Crestron.SimplSharpFramework;
using Crestron.SimplSharpFramework.Events;
using Crestron.SimplSharpFramework.Logging;
using Crestron.SimplSharpFramework.Utils;
using Crestron.SimplSharpPro;
using RGAPlugin.WinCe.ConfigClasses;

namespace RGAPlugin.WinCe.Fusion
{
    public class RoomSchedule : GenericBase, ICustomDevice
    {
        public FusionScheduler FusionScheduler;
        public FusionReporter FusionReporter;
        private ICustomMessage _customMessage;
        private readonly uint _networkId;
        private readonly CrestronControlSystem _controlControlSystem;
        private ILog _logger;
        private string _deviceId;

        private const string LogTag = "RM";

        public class RoomSchedulerInitData
        {
            public string RoomName { get; set; }
            public string FusionConnection { get; set; }
            public string MeetingRegEx { get; set; }
            public FusionConfig FusionConfig { get; set; }
            public StateController StateController { get; set; }
            public ILog Logger { get; set; }
            public ICustomMessage CustomMessage { get; set; }
            public string DeviceId { get; set; }
        }

        public RoomSchedule(uint networkId, CrestronControlSystem controlControlSystem)
        {
            _networkId = networkId;
            _controlControlSystem = controlControlSystem;

        }

        public void Initialize(string deviceId, GenericBase customDevice, ICustomMessage messageSender)
        {
            _customMessage = messageSender;
            _deviceId = deviceId;
        }

        public void Initialize(object customData)
        {
            var initdata = customData as RoomSchedulerInitData;

            if (initdata == null)
            {
                throw new MissingFieldException("Room Schedule Manager requires Custom Initdata 'RoomSchedulerInitData'");
            }

            _logger = initdata.Logger;
            _customMessage = initdata.CustomMessage;
            _deviceId = initdata.DeviceId;

            FusionScheduler = new FusionScheduler(_logger,_networkId, _controlControlSystem,
                initdata.RoomName,
                initdata.FusionConnection,
                initdata.MeetingRegEx);

            FusionScheduler.OnlineStatusChange += ReportOnlineStatus;
            FusionScheduler.MeetingUpdate += MeetingUpdate;

            FusionReporter = new FusionReporter(_logger, initdata.FusionConfig, initdata.StateController, FusionScheduler, _customMessage);
            FusionReporter.Initialize();

            _logger.InfoFormat(":{0}: Room Scheduling.  Room:{1}  Connection:{2}", LogTag, initdata.RoomName, initdata.FusionConnection);
        }

        private void MeetingUpdate(MeetingDetails md, FusionScheduler.MeetingUpdateStatus status)
        {
           _logger.InfoFormat(":{0}: Status:{1}  Start:{2} End:{3} Organizer:{4} Subject:{5}",LogTag, status,

               md.DateTimeStart, md.DateTimeEnd, md.Organizer, md.Subject);
           _customMessage.SendMessage(_deviceId,status.ToString(), string.Empty,md);
        }

       public void CreateAdhocMeeting(string organizer, int timeSpan)
       {
          FusionScheduler.CreateMeeting(organizer, timeSpan);
       }

       public List <int> CreateAdhocMeetingLengths(int availableHrs, int availableMins)
       {
         if (availableHrs > 0)
            return (new List<int> { 15, 30, 45, 60 });

         if (availableMins < 15)
            return (new List<int>());

         if ( availableMins < 30)
            return (new List <int> {15});

         if (availableMins < 45)
            return (new List<int> { 15, 30 });

         if (availableMins >= 45)
            return (new List<int> { 15, 30, 45, 60});

         return (new List <int> ());
       }

        private void ReportOnlineStatus(bool onlineStatus)
        {
            if (OnlineStatusChange == null) return;

            var change = OnlineStatusChange;

            change(this, new OnlineOfflineEventArgs(onlineStatus));
        }

        protected override void Dispose(bool paramDisposing)
        {
            FusionScheduler.Dispose(paramDisposing);
        }

        public override eDeviceRegistrationUnRegistrationResponse Register()
        {
            return FusionScheduler.IsRegistered
                ? eDeviceRegistrationUnRegistrationResponse.Success
                : eDeviceRegistrationUnRegistrationResponse.Failure;
        }

        public override eDeviceRegistrationUnRegistrationResponse UnRegister()
        {
            return FusionScheduler.UnRegister();
        }

        public override bool Disposed
        {
            get { throw new NotImplementedException(); }
        }

        protected override CrestronDeviceWithEvents SIMPLDeviceImplementation { get; set; }

        public override eDeviceType Type
        {
            get { return eDeviceType.Fusion; }
        }

        public override ReadOnlyCollection<ConnectedIpInformation> ConnectedIpList
        {
            get { return null; }
        }

        public override bool IsOnline
        {
            get { return FusionScheduler.IsOnline; }
        }

        public override bool Registered
        {
            get { return FusionScheduler.IsOnline; }
        }

        public override eDeviceRegistrationUnRegistrationFailureReason RegistrationFailureReason
        {
            get { throw new NotImplementedException(); }
        }

        public override eDeviceRegistrationUnRegistrationFailureReason UnRegistrationFailureReason
        {
            get { throw new NotImplementedException(); }
        }

        public override uint ID
        {
            get { return _networkId; }
        }

    
        public override event IpInformationChangeEventHandler IpInformationChange;
        public override event OnlineStatusChangeEventHandler OnlineStatusChange;


    }
}