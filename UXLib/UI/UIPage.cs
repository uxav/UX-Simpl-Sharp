using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UIPage : UIViewBase
    {
        public UIPage(BoolInputSig visibleDigitalJoin, BoolOutputSig visibleFeedbackJoin)
            : base(visibleDigitalJoin)
        {
            this.VisibleFeedbackJoin = visibleFeedbackJoin;
        }

        public UIPage(BoolInputSig visibleDigitalJoin, BoolOutputSig visibleFeedbackJoin, UILabel titleLabel, string title)
            : base(visibleDigitalJoin, titleLabel)
        {
            this.VisibleFeedbackJoin = visibleFeedbackJoin;
            this.Title = title;
        }

        private new void Hide() { }

        public override void Show()
        {
            if (base.Visible)
            {
                base.VisibleDigitalJoin.BoolValue = false;
                base.VisibleDigitalJoin.BoolValue = true;
            }
            else
                base.Show();
        }

        public UIPage(UIController uiController, uint visibleJoinNumber)
            : this(uiController.Device.BooleanInput[visibleJoinNumber], uiController.Device.BooleanOutput[visibleJoinNumber]) { }

        public UIPage(UIController uiController, uint visibleJoinNumber, UILabel titleLabel, string title)
            : this(uiController.Device.BooleanInput[visibleJoinNumber], uiController.Device.BooleanOutput[visibleJoinNumber], titleLabel, title) { }

        public UIPage(UIViewController viewController, uint visibleJoinNumber)
            : this(viewController.UIController.Device.BooleanInput[visibleJoinNumber], viewController.UIController.Device.BooleanOutput[visibleJoinNumber]) { }

        public UIPage(UIViewController viewController, uint visibleJoinNumber, UILabel titleLabel, string title)
            : this(viewController.UIController.Device.BooleanInput[visibleJoinNumber], viewController.UIController.Device.BooleanOutput[visibleJoinNumber], titleLabel, title) { }
    }
}