using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class Camera
    {
        internal Camera(CiscoCodec codec, int id, bool connected, string macAddress, string manufacturer, string model, string serialNumber, string softwareID)
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

        public int ID { get; protected set; }
        public bool Connected { get; protected set; }
        public string MacAddress { get; protected set; }
        public string Manufacturer { get; protected set; }
        public string Model { get; protected set; }
        public string SerialNumber { get; protected set; }
        public string SoftwareID { get; protected set; }

        public void Ramp(CameraPanCommand panCommand)
        {
            CommandArgs args = new CommandArgs("CameraId", this.ID);
            args.Add("Pan", panCommand.ToString());
            this.Ramp(args);
        }

        public void Ramp(CameraPanCommand panCommand, int speed)
        {
            CommandArgs args = new CommandArgs("CameraId", this.ID);
            args.Add("Pan", panCommand.ToString());
            args.Add("PanSpeed", speed);
            this.Ramp(args);
        }

        public void Ramp(CameraTiltCommand tiltCommand)
        {
            CommandArgs args = new CommandArgs("CameraId", this.ID);
            args.Add("Tilt", tiltCommand.ToString());
            this.Ramp(args);
        }

        public void Ramp(CameraTiltCommand tiltCommand, int speed)
        {
            CommandArgs args = new CommandArgs("CameraId", this.ID);
            args.Add("Tilt", tiltCommand.ToString());
            args.Add("TiltSpeed", speed);
            this.Ramp(args);
        }

        public void Ramp(CameraZoomCommand zoomCommand)
        {
            CommandArgs args = new CommandArgs("CameraId", this.ID);
            args.Add("Zoom", zoomCommand.ToString());
            this.Ramp(args);
        }

        public void Ramp(CameraZoomCommand zoomCommand, int speed)
        {
            CommandArgs args = new CommandArgs("CameraId", this.ID);
            args.Add("Zoom", zoomCommand.ToString());
            args.Add("ZoomSpeed", speed);
            this.Ramp(args);
        }

        public void Ramp(CameraFocusCommand focusCommand)
        {
            CommandArgs args = new CommandArgs("CameraId", this.ID);
            args.Add("Focus", focusCommand.ToString());
            this.Ramp(args);
        }

        public void Ramp(CameraFocusCommand focusCommand, int speed)
        {
            CommandArgs args = new CommandArgs("CameraId", this.ID);
            args.Add("Focus", focusCommand.ToString());
            args.Add("FocusSpeed", speed);
            this.Ramp(args);
        }

        void Ramp(CommandArgs args)
        {
            this.Codec.SendCommand("Camera/Ramp", args);
        }

        public void PanTiltReset()
        {
            this.Codec.SendCommand("Camera/PanTiltReset", new CommandArgs("CameraId", this.ID));
        }

        public void PresetActivate(int presetId)
        {
            this.Codec.SendCommand("Camera/Preset/Activate", new CommandArgs("PresetId", presetId));
        }
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