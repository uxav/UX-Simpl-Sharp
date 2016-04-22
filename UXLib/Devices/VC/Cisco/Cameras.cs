using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.VC.Cisco
{
    public class Cameras
    {
        public Cameras(CiscoCodec codec)
        {
            this.Codec = codec;
        }

        CiscoCodec Codec { get; set; }

        public void Ramp(int camera, CameraPanCommand panCommand)
        {
            CommandArgs args = new CommandArgs("CameraId", camera);
            args.Add("Pan", panCommand.ToString());
            this.Ramp(args);
        }

        public void Ramp(int camera, CameraPanCommand panCommand, int speed)
        {
            CommandArgs args = new CommandArgs("CameraId", camera);
            args.Add("Pan", panCommand.ToString());
            args.Add("PanSpeed", speed);
            this.Ramp(args);
        }

        public void Ramp(int camera, CameraTiltCommand tiltCommand)
        {
            CommandArgs args = new CommandArgs("CameraId", camera);
            args.Add("Tilt", tiltCommand.ToString());
            this.Ramp(args);
        }

        public void Ramp(int camera, CameraTiltCommand tiltCommand, int speed)
        {
            CommandArgs args = new CommandArgs("CameraId", camera);
            args.Add("Tilt", tiltCommand.ToString());
            args.Add("TiltSpeed", speed);
            this.Ramp(args);
        }

        public void Ramp(int camera, CameraZoomCommand zoomCommand)
        {
            CommandArgs args = new CommandArgs("CameraId", camera);
            args.Add("Zoom", zoomCommand.ToString());
            this.Ramp(args);
        }

        public void Ramp(int camera, CameraZoomCommand zoomCommand, int speed)
        {
            CommandArgs args = new CommandArgs("CameraId", camera);
            args.Add("Zoom", zoomCommand.ToString());
            args.Add("ZoomSpeed", speed);
            this.Ramp(args);
        }

        public void Ramp(int camera, CameraFocusCommand focusCommand)
        {
            CommandArgs args = new CommandArgs("CameraId", camera);
            args.Add("Focus", focusCommand.ToString());
            this.Ramp(args);
        }

        public void Ramp(int camera, CameraFocusCommand focusCommand, int speed)
        {
            CommandArgs args = new CommandArgs("CameraId", camera);
            args.Add("Focus", focusCommand.ToString());
            args.Add("FocusSpeed", speed);
            this.Ramp(args);
        }

        void Ramp(CommandArgs args)
        {
            this.Codec.SendCommand("Camera/Ramp", args);
        }

        public void PanTiltReset(int camera)
        {
            this.Codec.SendCommand("Camera/PanTiltReset", new CommandArgs("CameraId", camera));
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