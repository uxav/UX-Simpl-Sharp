using Crestron.SimplSharpPro;

namespace UXLib.UI
{
    public class UIActionSheetButton : UIButton
    {
        public ActionSheetButtonAction Action;

        public UIActionSheetButton(BoolOutputSig digitalPressJoin,
            BoolInputSig digitalFeedbackJoin, StringInputSig serialJoinSig, ActionSheetButtonAction action)
            : base(digitalPressJoin, digitalFeedbackJoin, serialJoinSig)
        {
            this.Action = action;
        }
    }

    public enum ActionSheetButtonAction
    {
        Cancel,
        Destruct,
        Confirm,
        TimedOut
    }
}