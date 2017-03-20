using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Models;

namespace UXLib.Devices.ParitionSensors
{
    public interface IPartitionSensor
    {
        uint ID { get; }
        Room ParentRoom { get; }
        Room ChildRoom { get; }
        RoomPartitionState State { get; }
        event PartitionChangeEventHandler StateChanged;
    }

    public delegate void PartitionChangeEventHandler(IPartitionSensor system, PartitionEventArgs args);

    public class PartitionEventArgs : EventArgs
    {
        public Room ParentRoom;
        public Room ChildRoom;
        public RoomPartitionState PartitionState;
        public PartitionEventArgs(Room parentRoom, Room childRoom, RoomPartitionState state)
        {
            ParentRoom = parentRoom;
            ChildRoom = childRoom;
            PartitionState = state;
        }
    }

    public enum RoomPartitionState
    {
        Closed,
        Open
    }
}