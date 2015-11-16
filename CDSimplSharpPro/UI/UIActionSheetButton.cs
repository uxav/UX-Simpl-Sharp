using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace CDSimplSharpPro.UI
{
    public class UIActionSheetButton : UIButton
    {
        public eActionSheetButtonAction Action;

        public UIActionSheetButton(BoolOutputSig digitalPressJoin,
            BoolInputSig digitalFeedbackJoin, StringInputSig serialJoinSig, eActionSheetButtonAction action)
            : base(digitalPressJoin, digitalFeedbackJoin, serialJoinSig)
        {
            this.Action = action;
        }
    }

    public enum eActionSheetButtonAction
    {
        Cancel,
        Destruct,
        Confirm
    }
}