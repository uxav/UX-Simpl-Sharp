using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace UXLib.UI
{
    public class UISmartObjectCheckboxList : UISmartObject
    {
        private ListData Data;
        public ushort MaxNumberOfItems { get; private set; }

        public UISmartObjectCheckboxList(SmartObject smartObject, ListData listData, BoolInputSig enableJoin, BoolInputSig visibleJoin)
            : base(smartObject, enableJoin, visibleJoin)
        {
            uint item = 1;
            this.Data = listData;
            this.Data.DataChange += new ListDataChangeEventHandler(Data_DataChange);
            try
            {
                while (smartObject.BooleanOutput.Contains(string.Format("Item {0} Checked", item)))
                {
                    UISmartObjectButton listButton = new UISmartObjectButton(
                        item, this.DeviceSmartObject,
                        string.Format("Item {0} Checked", item),
                        string.Format("Item {0} Checked", item),
                        string.Format("Item {0} Text", item)
                        );
                    this.AddButton(listButton);

                    item++;
                }

                this.MaxNumberOfItems = (ushort)(item - 1);
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error constructing UISmartObjectList with KeyName: {0}", e.Message);
            }
        }

        void Data_DataChange(ListData listData, ListDataChangeEventArgs args)
        {
            if (args.EventType == eListDataChangeEventType.IsStartingToLoad)
            {
                this.Disable();
            }
            else if (args.EventType == eListDataChangeEventType.HasCleared)
            {
                this.Disable();
            }
            else if (args.EventType == eListDataChangeEventType.HasLoaded)
            {
                ushort listSize;

                if (listData.Count > this.MaxNumberOfItems)
                {
                    listSize = this.MaxNumberOfItems;
                }
                else
                {
                    listSize = (ushort)listData.Count;
                }

                for (uint item = 1; item <= MaxNumberOfItems; item++)
                {
                    if (item <= listSize)
                    {
                        int listDataIndex = (int)item - 1;
                        this.Buttons[item].Title = listData[listDataIndex].Title;
                        this.Buttons[item].LinkedObject = listData[listDataIndex].DataObject;
                    }
                    else
                    {
                        this.Buttons[item].Title = "";
                    }
                }

                this.Enable();
            }
            else if (args.EventType == eListDataChangeEventType.ItemSelectionHasChanged)
            {
                for (uint item = 1; item <= listData.Count; item++)
                {
                    int listDataIndex = (int)item - 1;
                    this.Buttons[item].Feedback = listData[listDataIndex].IsSelected;
                }
            }
        }

        public object LinkedObjectForButton(uint buttonIndex)
        {
            return this.Buttons[buttonIndex].LinkedObject;
        }
    }
}