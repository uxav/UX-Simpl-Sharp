using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.ParitionSensors
{
    public class PartitionSensorCollection : UXLib.Models.UXCollection<IPartitionSensor>
    {
        public PartitionSensorCollection(IEnumerable<IPartitionSensor> sensors)
            : base(sensors) { }

        public override IPartitionSensor this[uint id]
        {
            get
            {
                return base[id];
            }
            internal set
            {
                base[id] = value;
            }
        }

        public PartitionSensorCollection ForParentRoom(UXLib.Models.Room room)
        {
            return new PartitionSensorCollection(this.ToList().Where(r => r.ParentRoom == room));
        }
    }
}