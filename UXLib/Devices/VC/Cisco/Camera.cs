using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using UXLib.Devices.Cameras;

namespace UXLib.Devices.VC.Cisco
{
    public class Camera : ICamera
    {
        internal Camera(CiscoCodec codec, uint id, bool connected, string macAddress, string manufacturer, string model, string serialNumber, string softwareID)
        {
            this.Codec = codec;
            this.ID = id;
            this.Connected = connected;
            this.MacAddress = macAddress;
            this.Manufacturer = manufacturer;
            this.Model = model;
            this.SerialNumber = serialNumber;
            this.SoftwareID = softwareID;
        }

        CiscoCodec Codec { get; set; }

        public uint ID { get; internal set; }
        public bool Connected { get; internal set; }
        public string MacAddress { get; internal set; }
        public string Manufacturer { get; internal set; }
        public string Model { get; internal set; }
        public string SerialNumber { get; internal set; }
        public string SoftwareID { get; internal set; }

        public void Ramp(CameraPanCommand panCommand)
        {
            CommandArgs args = new CommandArgs("CameraId", (int)this.ID);
            args.Add("Pan", panCommand.ToString());
            this.Ramp(args);
        }

        public void Ramp(CameraPanCommand panCommand, int speed)
        {
            CommandArgs args = new CommandArgs("CameraId", (int)this.ID);
            args.Add("Pan", panCommand.ToString());
            args.Add("PanSpeed", speed);
            this.Ramp(args);
        }

        public void Ramp(CameraTiltCommand tiltCommand)
        {
            CommandArgs args = new CommandArgs("CameraId", (int)this.ID);
            args.Add("Tilt", tiltCommand.ToString());
            this.Ramp(args);
        }

        public void Ramp(CameraTiltCommand tiltCommand, int speed)
        {
            CommandArgs args = new CommandArgs("CameraId", (int)this.ID);
            args.Add("Tilt", tiltCommand.ToString());
            args.Add("TiltSpeed", speed);
            this.Ramp(args);
        }

        public void Ramp(CameraZoomCommand zoomCommand)
        {
            CommandArgs args = new CommandArgs("CameraId", (int)this.ID);
            args.Add("Zoom", zoomCommand.ToString());
            this.Ramp(args);
        }

        public void Ramp(CameraZoomCommand zoomCommand, int speed)
        {
            CommandArgs args = new CommandArgs("CameraId", (int)this.ID);
            args.Add("Zoom", zoomCommand.ToString());
            args.Add("ZoomSpeed", speed);
            this.Ramp(args);
        }

        public void Ramp(CameraFocusCommand focusCommand)
        {
            CommandArgs args = new CommandArgs("CameraId", (int)this.ID);
            args.Add("Focus", focusCommand.ToString());
            this.Ramp(args);
        }

        public void Ramp(CameraFocusCommand focusCommand, int speed)
        {
            CommandArgs args = new CommandArgs("CameraId", (int)this.ID);
            args.Add("Focus", focusCommand.ToString());
            args.Add("FocusSpeed", speed);
            this.Ramp(args);
        }

        void Ramp(CommandArgs args)
        {
            this.Codec.SendCommand("Camera/Ramp", args);
        }

        public void PresetActivateDefaultPosition()
        {
            this.Codec.SendCommand("Camera/Preset/ActivateDefaultPosition", new CommandArgs("CameraId", (int)this.ID));
        }

        public void PresetActivate(int presetId)
        {
            this.Codec.SendCommand("Camera/Preset/Activate", new CommandArgs("PresetId", presetId));
        }

        #region ICamera Members

        public void TiltUp()
        {
            this.Ramp(CameraTiltCommand.Up);
        }

        public void TiltDown()
        {
            this.Ramp(CameraTiltCommand.Down);
        }

        public void TiltStop()
        {
            this.Ramp(CameraTiltCommand.Stop);
        }

        public void PanLeft()
        {
            this.Ramp(CameraPanCommand.Left);
        }

        public void PanRight()
        {
            this.Ramp(CameraPanCommand.Right);
        }

        public void PanStop()
        {
            this.Ramp(CameraPanCommand.Stop);
        }

        public void ZoomIn()
        {
            this.Ramp(CameraZoomCommand.In);
        }

        public void ZoomOut()
        {
            this.Ramp(CameraZoomCommand.Out);
        }

        public void ZoomStop()
        {
            this.Ramp(CameraZoomCommand.Stop);
        }

        public void Home()
        {
            this.PresetActivateDefaultPosition();
        }

        public string Name
        {
            get
            {
                return string.Format("Camera {0}", this.ID);
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string DeviceManufacturer
        {
            get { return this.Manufacturer; }
        }

        public string DeviceModel
        {
            get { return this.Model; }
        }

        public string DeviceSerialNumber
        {
            get { return this.SerialNumber; }
        }

        #endregion
    }

    public enum CameraPanCommand
    {
        Left,
        Right,
        Stop
    }

    public enum CameraTiltCommand
    {
        Down,
        Up,
        Stop
    }

    public enum CameraZoomCommand
    {
        In,
        Out,
        Stop
    }

    public enum CameraFocusCommand
    {
        Far,
        Near,
        Stop
    }
}