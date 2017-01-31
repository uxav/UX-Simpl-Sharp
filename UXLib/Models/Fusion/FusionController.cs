using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;

namespace UXLib.Models.Fusion
{
    public class FusionController
    {
        public FusionController(UXLib.Models.Room room, uint ipId, bool useScheduling)
        {
            this.Room = room;
            this.FusionRoom = new FusionRoom(ipId, room.ControlSystem, room.Name, Guid.NewGuid().ToString());
            this.FusionRoom.OnlineStatusChange += new OnlineStatusChangeEventHandler(FusionRoom_OnlineStatusChange);
            this.Assets = new FusionAssetCollection(this);
            this.FusionRoom.FusionStateChange += new FusionStateEventHandler(FusionRoom_FusionStateChange);
            if (useScheduling)
            {
                this.FusionRoom.ExtenderRoomViewSchedulingDataReservedSigs.Use();
                this.Scheduler = new FusionScheduler(this);
            }
        }

        public UXLib.Models.Room Room { get; private set; }
        public FusionRoom FusionRoom { get; private set; }
        public FusionAssetCollection Assets { get; private set; }
        public FusionScheduler Scheduler { get; private set; }

        public void Register()
        {
            if (this.FusionRoom.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                ErrorLog.Error("Could not register instance of Fusion with IP ID 0x{0}", FusionRoom.ID.ToString("X2"));
        }

        void FusionRoom_OnlineStatusChange(GenericBase currentDevice, OnlineOfflineEventArgs args)
        {
            this.Room.FusionUpdate();
        }

        public void SetSystemPowerStatus(bool status)
        {
            this.FusionRoom.SystemPowerOn.InputSig.BoolValue = status;
        }

        public void SetDisplayPowerStatus(bool status)
        {
            this.FusionRoom.DisplayPowerOn.InputSig.BoolValue = status;
        }

        void FusionRoom_FusionStateChange(FusionBase device, FusionStateEventArgs args)
        {
            switch (args.EventId)
            {
                case FusionEventIds.SystemPowerOffReceivedEventId:
                    if (this.FusionRoom.SystemPowerOff.OutputSig.BoolValue)
                        this.Room.FusionSystemPowerRequest(false);
                    break;
                case FusionEventIds.SystemPowerOnReceivedEventId:
                    if (this.FusionRoom.SystemPowerOn.OutputSig.BoolValue)
                        this.Room.FusionSystemPowerRequest(true);
                    break;
            }
        }
    }
}