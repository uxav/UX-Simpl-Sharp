using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UIButton : UIButtonBase
    {
        public UIButton(BoolOutputSig digitalPressJoin)
            : base(digitalPressJoin)
        {
            this.Device.SigChange += new SigEventHandler(Device_SigChange);
        }

        public UIButton(BoolOutputSig digitalPressJoin, StringInputSig serialJoinSig)
            : base(digitalPressJoin, serialJoinSig)
        {
            this.Device.SigChange += new SigEventHandler(Device_SigChange);
        }

        public UIButton(BoolOutputSig digitalPressJoin, BoolInputSig digitalFeedbackJoin)
            : base(digitalPressJoin, digitalFeedbackJoin)
        {
            this.Device.SigChange += new SigEventHandler(Device_SigChange);
        }
        
        public UIButton(BoolOutputSig digitalPressJoin, BoolInputSig digitalFeedbackJoin,
            StringInputSig serialJoinSig)
            : base(digitalPressJoin, digitalFeedbackJoin, serialJoinSig)
        {
            this.Device.SigChange += new SigEventHandler(Device_SigChange);
        }

        public UIButton(BoolOutputSig digitalPressJoin, BoolInputSig digitalFeedbackJoin,
            StringInputSig serialJoinSig, BoolInputSig enableJoinSig, BoolInputSig visibleJoinSig)
            : base(digitalPressJoin, digitalFeedbackJoin, serialJoinSig, enableJoinSig, visibleJoinSig)
        {
            this.Device.SigChange += new SigEventHandler(Device_SigChange);
        }

        void Device_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            if (args.Sig.Type == eSigType.Bool && args.Sig.Number == this.JoinNumber)
            {
                this.Down = args.Sig.BoolValue;
            }
        }

        public void Dispose()
        {
            this.Device.SigChange -= new SigEventHandler(Device_SigChange);
        }
    }
}