using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro.UI
{
    public class UISmartObjectButton : UIButtonBase
    {
        public SmartObject SmartObject;
        StringInputSig IconJoin;

        public string Icon
        {
            set
            {
                if (this.IconJoin != null)
                    this.IconJoin.StringValue = value;
            }
            get
            {
                if (this.IconJoin != null)
                    return IconJoin.StringValue;
                return null;
            }
        }

        public uint ItemIndex { get; private set; }

        public UISmartObjectButton(uint itemIndex, SmartObject smartObject, string digitalPressJoinName)
            : base(smartObject.BooleanOutput[digitalPressJoinName])
        {
            this.ItemIndex = itemIndex;
            this.SmartObject = smartObject;
            this.SmartObject.SigChange += new SmartObjectSigChangeEventHandler(SmartObject_SigChange);
        }

        public UISmartObjectButton(uint itemIndex, SmartObject smartObject, string digitalPressJoinName,
            string digitalFeedbackJoinName)
            : base(smartObject.BooleanOutput[digitalPressJoinName],
            smartObject.BooleanInput[digitalFeedbackJoinName])
        {
            this.ItemIndex = itemIndex;
            this.SmartObject = smartObject;
            this.SmartObject.SigChange += new SmartObjectSigChangeEventHandler(SmartObject_SigChange);
        }

        public UISmartObjectButton(uint itemIndex, SmartObject smartObject, string digitalPressJoinName,
            string digitalFeedbackJoinName, string titleJoinName)
            : base(smartObject.BooleanOutput[digitalPressJoinName],
            smartObject.BooleanInput[digitalFeedbackJoinName], smartObject.StringInput[titleJoinName])
        {
            this.ItemIndex = itemIndex;
            this.SmartObject = smartObject;
            this.SmartObject.SigChange += new SmartObjectSigChangeEventHandler(SmartObject_SigChange);
        }

        public UISmartObjectButton(uint itemIndex, SmartObject smartObject, string digitalPressJoinName,
            string digitalFeedbackJoinName, string titleJoinName, string iconJoinSigName)
            : base(smartObject.BooleanOutput[digitalPressJoinName],
            smartObject.BooleanInput[digitalFeedbackJoinName], smartObject.StringInput[titleJoinName])
        {
            this.ItemIndex = itemIndex;
            this.SmartObject = smartObject;
            this.IconJoin = smartObject.StringInput[iconJoinSigName];
            this.SmartObject.SigChange += new SmartObjectSigChangeEventHandler(SmartObject_SigChange);
        }

        public UISmartObjectButton(uint itemIndex, SmartObject smartObject, string digitalPressJoinName,
            string digitalFeedbackJoinName, string titleJoinName, string iconJoinSigName,
            string enableJoinSigName, string visibleJoinSigName)
            : base(smartObject.BooleanOutput[digitalPressJoinName],
            smartObject.BooleanInput[digitalFeedbackJoinName], smartObject.StringInput[titleJoinName],
            smartObject.BooleanInput[enableJoinSigName], smartObject.BooleanInput[visibleJoinSigName])
        {
            this.ItemIndex = itemIndex;
            this.SmartObject = smartObject;
            this.IconJoin = smartObject.StringInput[iconJoinSigName];
            this.SmartObject.SigChange += new SmartObjectSigChangeEventHandler(SmartObject_SigChange);
        }

        void SmartObject_SigChange(GenericBase currentDevice, SmartObjectEventArgs args)
        {
            if (args.Sig.Type == eSigType.Bool && args.Sig.Number == this.JoinNumber)
            {
                this.Down = args.Sig.BoolValue;
            }
        }

        public override void Dipose()
        {
            base.Dipose();
            this.SmartObject.SigChange -= new SmartObjectSigChangeEventHandler(SmartObject_SigChange);
        }
    }
}