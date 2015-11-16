using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro.UI
{
    public class UIPage : UIViewBase
    {
        public BoolOutputSig VisibleJoinFeedbackSig { get; protected set; }

        public UIPage(BoolInputSig visibleJoinSig, BoolOutputSig visibleJoinFeedbackSig)
            : base(visibleJoinSig)
        {
            VisibleJoinFeedbackSig = visibleJoinFeedbackSig;
            BasicTriList device = VisibleJoinFeedbackSig.Owner as BasicTriList;
            device.SigChange += new SigEventHandler(device_SigChange);
        }

        public UIPage(BoolInputSig visibleJoinSig, BoolOutputSig visibleJoinFeedbackSig, UILabel titleLabel, string title)
            : base(visibleJoinSig, titleLabel, title)
        {
            VisibleJoinFeedbackSig = visibleJoinFeedbackSig;
            BasicTriList device = VisibleJoinFeedbackSig.Owner as BasicTriList;
            device.SigChange += new SigEventHandler(device_SigChange);
        }

        public override bool Visible
        {
            get
            {
                return this.VisibleJoinFeedbackSig.BoolValue;
            }
        }

        void device_SigChange(BasicTriList currentDevice, SigEventArgs args)
        {
            if (args.Sig.Type == eSigType.Bool && args.Sig.Number == VisibleJoinFeedbackSig.Number)
            {
                if (args.Sig.BoolValue)
                    this.Show();
                else
                    this.Hide();
            }
        }

        public override void Dispose()
        {
            base.Dispose();

            BasicTriList device = VisibleJoinFeedbackSig.Owner as BasicTriList;
            device.SigChange -= new SigEventHandler(device_SigChange);
        }
    }
}