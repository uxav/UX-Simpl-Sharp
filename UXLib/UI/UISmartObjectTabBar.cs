using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UISmartObjectTabBar : UISmartObject
    {
        public UISmartObjectTabBar(SmartObject smartObject)
            : base(smartObject)
        {
            uint item = 1;
            try
            {
                while (smartObject.BooleanOutput.Contains(string.Format("Tab Button {0} Press", item)))
                {
                    UISmartObjectButton tabButton = new UISmartObjectButton(this,
                        item, this.DeviceSmartObject,
                        string.Format("Tab Button {0} Press", item),
                        string.Format("Tab Button {0} Select", item)
                        );
                    this.AddButton(tabButton);

                    item++;
                }

                this.NumberOfItems = (ushort)(item - 1);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error constructing UISmartObjectTabBar with KeyName: {0}", e.Message);
            }
        }
        
        public UISmartObjectTabBar(SmartObject smartObject, BoolInputSig objectEnableJoin, BoolInputSig objectVisibleJoin)
            : this(smartObject)
        {
            this.EnableJoin = objectEnableJoin;
            this.VisibleJoin = objectVisibleJoin;
        }

        public void SetTabAsSelected(uint index)
        {
            foreach (UISmartObjectButton button in this.Buttons)
            {
                if (button.ItemIndex != index)
                {
                    button.Feedback = false;
                }
            }

            if (index > 0 && index <= this.NumberOfItems)
                this.Buttons[index].Feedback = true;
        }
    }
}