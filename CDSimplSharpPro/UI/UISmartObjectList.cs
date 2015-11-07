using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro.UI
{
    public class UISmartObjectList : UISmartObject
    {
        public ushort NumberOfItems
        {
            set
            {
                this.DeviceSmartObject.UShortInput["Set Number of Items"].UShortValue = value;
            }
            get
            {
                return this.DeviceSmartObject.UShortInput["Set Number of Items"].UShortValue;
            }
        }
        public ushort ScrollToItem
        {
            set
            {
                this.DeviceSmartObject.UShortInput["Scroll To Item"].UShortValue = value;
            }
        }
        public UISmartObjectList(string keyName, SmartObject smartObject)
            : base(keyName, smartObject)
        {
            uint item = 1;
            try
            {
                while (smartObject.BooleanOutput.Contains(string.Format("Item {0} Pressed", item)))
                {
                    UIButton listButton = new UIButton(
                        string.Format("List Button {0}", item),
                        smartObject.BooleanOutput[string.Format("Item {0} Pressed", item)],
                        smartObject.BooleanInput[string.Format("Item {0} Selected", item)],
                        smartObject.StringInput[string.Format("Set Item {0} Text", item)],
                        smartObject.BooleanInput[string.Format("Item {0} Enabled", item)],
                        smartObject.BooleanInput[string.Format("Item {0} Visible", item)]
                        );
                    this.AddButton(listButton);

                    item++;
                }

                this.NumberOfItems = (ushort)item;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error constructing UISmartObjectList with KeyName: {0}", e.Message);
            }
        }
    }
}