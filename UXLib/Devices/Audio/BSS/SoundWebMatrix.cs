using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Devices.Audio.BSS
{
    public class SoundWebMatrix : SoundWebObject
    {
        public SoundWebMatrix(SoundWeb device, string address, uint inputs, uint outputs)
        {
            this.Device = device;
            InputCount = inputs;
            OutputCount = outputs;
            HiQAddress = address;
            OutputValues = new Dictionary<uint, uint>();
            for (uint output = 1; output <= outputs; output++)
            {
                OutputValues.Add(output, 0);
            }
            this.FeedbackReceived += new SoundWebObjectFeedbackEventHandler(SoundWebMatrix_FeedbackReceived);
        }

        public uint InputCount { get; private set; }
        public uint OutputCount { get; private set; }
        Dictionary<uint, uint> OutputValues;

        public void Route(uint output, uint input)
        {
            string paramID = "\x00" + (char)(output - 1);
            string value = "\x00\x00\x00" + (char)input;

            Device.Socket.Send("\x88", HiQAddress, paramID, value);

            if (OutputValues[output] != input)
            {
                OutputValues[output] = input;
                OnOutputChange(output, input);
            }
        }

        public override void Subscribe()
        {
            base.Subscribe();
            string paramID = "\x00\x00";
            string value = "\x00\x00\x00\x00";
            Device.Socket.Send("\x89", HiQAddress, paramID, value);
        }

        void SoundWebMatrix_FeedbackReceived(SoundWebObject soundWebObject, SoundWebObjectFeedbackEventArgs args)
        {
            this.OutputValues[(uint)args.ParamID + 1] = (uint)args.Value;
            OnOutputChange((uint)args.ParamID + 1, (uint)args.Value);
        }

        public event SoundWebMatrixOutputChangeHandler OutputChange;

        protected virtual void OnOutputChange(uint output, uint input)
        {
            if (OutputChange != null)
            {
                OutputChange(this, new SoundWebMatrixOutputChangeEventArgs(output, input));
            }
        }

        public uint this[uint output]
        {
            get
            {
                return OutputValues[output];
            }
            set
            {
                this.Route(output, value);
            }
        }
    }

    public delegate void SoundWebMatrixOutputChangeHandler(SoundWebMatrix soundWebMatrix, SoundWebMatrixOutputChangeEventArgs args);

    public class SoundWebMatrixOutputChangeEventArgs
    {
        public SoundWebMatrixOutputChangeEventArgs(uint output, uint input)
        {
            Output = output;
            Input = input;
        }

        public uint Output;
        public uint Input;
    }
}