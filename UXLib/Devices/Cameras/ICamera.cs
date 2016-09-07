using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Cameras
{
    public interface ICamera : IDevice
    {
        void TiltUp();
        void TiltDown();
        void TiltStop();
        void PanLeft();
        void PanRight();
        void PanStop();
        void ZoomIn();
        void ZoomOut();
        void ZoomStop();
        void Home();
    }
}