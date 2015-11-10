using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro.UI
{
    public class UISmartObjectButton : UIButton
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
            : base(new UIKey(digitalPressJoinName, itemIndex), smartObject.BooleanOutput[digitalPressJoinName])
        {
            this.ItemIndex = itemIndex;
            this.SmartObject = smartObject;
        }

        public UISmartObjectButton(uint itemIndex, SmartObject smartObject, string digitalPressJoinName,
            string digitalFeedbackJoinName)
            : base(new UIKey(digitalPressJoinName, itemIndex), smartObject.BooleanOutput[digitalPressJoinName],
            smartObject.BooleanInput[digitalFeedbackJoinName])
        {
            this.ItemIndex = itemIndex;
            this.SmartObject = smartObject;
        }

        public UISmartObjectButton(uint itemIndex, SmartObject smartObject, string digitalPressJoinName,
            string digitalFeedbackJoinName, string titleJoinName)
            : base(new UIKey(digitalPressJoinName, itemIndex), smartObject.BooleanOutput[digitalPressJoinName],
            smartObject.BooleanInput[digitalFeedbackJoinName], smartObject.StringInput[titleJoinName])
        {
            this.ItemIndex = itemIndex;
            this.SmartObject = smartObject;
        }

        public UISmartObjectButton(uint itemIndex, SmartObject smartObject, string digitalPressJoinName,
            string digitalFeedbackJoinName, string titleJoinName, string iconJoinSigName)
            : base(new UIKey(digitalPressJoinName, itemIndex), smartObject.BooleanOutput[digitalPressJoinName],
            smartObject.BooleanInput[digitalFeedbackJoinName], smartObject.StringInput[titleJoinName])
        {
            this.ItemIndex = itemIndex;
            this.SmartObject = smartObject;
            this.IconJoin = smartObject.StringInput[iconJoinSigName];
        }

        public UISmartObjectButton(uint itemIndex, SmartObject smartObject, string digitalPressJoinName,
            string digitalFeedbackJoinName, string titleJoinName, string iconJoinSigName,
            string enableJoinSigName, string visibleJoinSigName)
            : base(new UIKey(digitalPressJoinName, itemIndex), smartObject.BooleanOutput[digitalPressJoinName],
            smartObject.BooleanInput[digitalFeedbackJoinName], smartObject.StringInput[titleJoinName],
            smartObject.BooleanInput[enableJoinSigName], smartObject.BooleanInput[visibleJoinSigName])
        {
            this.ItemIndex = itemIndex;
            this.SmartObject = smartObject;
            this.IconJoin = smartObject.StringInput[iconJoinSigName];
        }
    }
}