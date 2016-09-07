using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.Fusion;

namespace UXLib.Models
{
    public class Fusion
    {
        public Fusion(Room room, uint ipId)
        {
            this.Room = room;
            this.FusionRoom = new FusionRoom(ipId, room.ControlSystem, room.Name, Guid.NewGuid().ToString());
            this.Assets = new FusionAssetCollection(this);
        }

        public Room Room { get; private set; }
        public FusionRoom FusionRoom { get; private set; }
        public FusionAssetCollection Assets { get; private set; }
        public void Register()
        {
            if (this.FusionRoom.Register() != eDeviceRegistrationUnRegistrationResponse.Success)
                ErrorLog.Error("Could not register instance of Fusion with IP ID 0x{0}", FusionRoom.ID.ToString("X2"));
        }
    }
}