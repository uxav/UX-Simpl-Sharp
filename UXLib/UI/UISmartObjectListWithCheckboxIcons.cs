using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro.UI
{
    public class UISmartObjectListWithCheckboxIcons : UISmartObjectList
    {
        public UISmartObjectListWithCheckboxIcons(SmartObject smartObject, ListData listData, BoolInputSig enableJoin, BoolInputSig visibleJoin)
            : base(smartObject, listData, enableJoin, visibleJoin)
        {

        }

        public override void Data_DataChange(ListData listData, ListDataChangeEventArgs args)
        {
            if (args.EventType == eListDataChangeEventType.IsStartingToLoad)
            {
                this.Disable();
                this.Buttons[1].Title = "Loading...";
                this.Buttons[1].Icon = "Info";
                this.NumberOfItems = 1;
                if (LoadingSubPageOverlay != null)
                    LoadingSubPageOverlay.BoolValue = true;
            }
            else if (args.EventType == eListDataChangeEventType.HasCleared)
            {
                for (uint item = 1; item <= this.NumberOfItems; item++)
                {
                    this.Buttons[item].Feedback = false;
                }
                this.NumberOfItems = 0;
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

                this.NumberOfItems = listSize;

                for (uint item = 1; item <= listSize; item++)
                {
                    int listDataIndex = (int)item - 1;
                    this.Buttons[item].Title = listData[listDataIndex].Title;
                    if (listData[listDataIndex].IsSelected)
                        this.Buttons[item].Icon = UIMediaIcons.CheckboxChecked;
                    else
                        this.Buttons[item].Icon = UIMediaIcons.CheckboxOff;
                    this.Buttons[item].LinkedObject = listData[listDataIndex].DataObject;
                }

                this.Enable();
                if (LoadingSubPageOverlay != null)
                    LoadingSubPageOverlay.BoolValue = false;
            }
            else if (args.EventType == eListDataChangeEventType.ItemSelectionHasChanged)
            {
                for (uint item = 1; item <= this.NumberOfItems; item++)
                {
                    int listDataIndex = (int)item - 1;
                    if (listData[listDataIndex].IsSelected)
                        this.Buttons[item].Icon = UIMediaIcons.CheckboxChecked;
                    else
                        this.Buttons[item].Icon = UIMediaIcons.CheckboxOff;
                }
            }
        }
    }
}