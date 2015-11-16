using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro.UI
{
    public class UIButton : UIButtonBase
    {
        public UIButton(UIKey key, BoolOutputSig digitalPressJoin)
            : base(key, digitalPressJoin)
        {
            this.Device.SigChange += new SigEventHandler(Device_SigChange);
        }

        public UIButton(UIKey key, BoolOutputSig digitalPressJoin, StringInputSig serialJoinSig)
            : base(key, digitalPressJoin, serialJoinSig)
        {
            this.Device.SigChange += new SigEventHandler(Device_SigChange);
        }

        public UIButton(UIKey key, BoolOutputSig digitalPressJoin, BoolInputSig digitalFeedbackJoin)
            : base(key, digitalPressJoin, digitalFeedbackJoin)
        {
            this.Device.SigChange += new SigEventHandler(Device_SigChange);
        }
        
        public UIButton(UIKey key, BoolOutputSig digitalPressJoin, BoolInputSig digitalFeedbackJoin,
            StringInputSig serialJoinSig)
            : base(key, digitalPressJoin, digitalFeedbackJoin, serialJoinSig)
        {
            this.Device.SigChange += new SigEventHandler(Device_SigChange);
        }

        public UIButton(UIKey key, BoolOutputSig digitalPressJoin, BoolInputSig digitalFeedbackJoin,
            StringInputSig serialJoinSig, BoolInputSig enableJoinSig, BoolInputSig visibleJoinSig)
            : base(key, digitalPressJoin, digitalFeedbackJoin, serialJoinSig, enableJoinSig, visibleJoinSig)
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