using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace UXLib.UI
{
    public class UISmartObjectCheckboxList : UISmartObjectList
    {
        public UISmartObjectCheckboxList(SmartObject smartObject, ListData listData, BoolInputSig enableJoin, BoolInputSig visibleJoin)
            : base(smartObject, listData, enableJoin, visibleJoin)
        {
        }

        protected override void Data_DataChange(ListData listData, ListDataChangeEventArgs args)
        {
            if (args.EventType == eListDataChangeEventType.HasLoaded)
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
                    this.Buttons[item].Enabled = listData[listDataIndex].Enabled;
                }

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
            else
            {
                base.Data_DataChange(listData, args);
            }
        }
    }
}