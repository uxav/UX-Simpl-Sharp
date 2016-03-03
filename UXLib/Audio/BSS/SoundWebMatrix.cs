using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;

namespace UXLib.Audio.BSS
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
            OutputValues[output] = input;

            //CrestronConsole.PrintLine("Soundweb Matrix Input {0} to Output {1}", input, output);

            Device.Socket.Send("\x88", HiQAddress, paramID, value);
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
            this.OutputValues[(uint)args.ParamID] = (uint)args.Value;
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
}