using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace UXLib.UI
{
    public class UISmartObjectList : UISmartObject
    {
        private ListData Data;
        public ushort MaxNumberOfItems { get; private set; }
        protected BoolInputSig LoadingSubPageOverlay;

        public ushort NumberOfItems
        {
            set
            {
                if (this.DeviceSmartObject.UShortInput.Contains("Set Number of Items"))
                    this.DeviceSmartObject.UShortInput["Set Number of Items"].UShortValue = value;
            }
            get
            {
                if (this.DeviceSmartObject.UShortInput.Contains("Set Number of Items"))
                    return this.DeviceSmartObject.UShortInput["Set Number of Items"].UShortValue;
                return this.MaxNumberOfItems;
            }
        }

        public ushort ScrollToItem
        {
            set
            {
                this.DeviceSmartObject.UShortInput["Scroll To Item"].UShortValue = value;
            }
        }

        public bool IsMoving
        {
            get
            {
                if (this.DeviceSmartObject.BooleanOutput["Is Moving"] != null)
                    return this.DeviceSmartObject.BooleanOutput["Is Moving"].BoolValue;
                return false;
            }
        }

        public UISmartObjectList(SmartObject smartObject, ListData listData, BoolInputSig enableJoin, BoolInputSig visibleJoin)
            : base(smartObject, enableJoin, visibleJoin)
        {
            uint item = 1;
            this.Data = listData;
            this.Data.DataChange += new ListDataChangeEventHandler(Data_DataChange);
            try
            {
                while (smartObject.BooleanOutput.Contains(string.Format("Item {0} Pressed", item)))
                {
                    UISmartObjectButton listButton = new UISmartObjectButton(
                        item, this.DeviceSmartObject,
                        string.Format("Item {0} Pressed", item),
                        string.Format("Item {0} Selected", item),
                        string.Format("Set Item {0} Text", item),
                        string.Format("Set Item {0} Icon Serial", item),
                        string.Format("Item {0} Enabled", item),
                        string.Format("Item {0} Visible", item)
                        );
                    this.AddButton(listButton);

                    item++;
                }

                this.MaxNumberOfItems = (ushort)(item - 1);
                this.NumberOfItems = 0;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error constructing UISmartObjectList with KeyName: {0}", e.Message);
            }
        }

        public virtual void Data_DataChange(ListData listData, ListDataChangeEventArgs args)
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
                    this.Buttons[item].Icon = listData[listDataIndex].Icon;
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
                    this.Buttons[item].Feedback = listData[listDataIndex].IsSelected;
                }
            }
        }

        public object LinkedObjectForButton(uint buttonIndex)
        {
            return this.Buttons[buttonIndex].LinkedObject;
        }

        public void LoadingSubPageOverlayAssign(BoolInputSig loadingSubPageOverlaySig)
        {
            this.LoadingSubPageOverlay = loadingSubPageOverlaySig;
        }

        public override void Dispose()
        {
            base.Dispose();
            this.Data.DataChange -= new ListDataChangeEventHandler(Data_DataChange);
        }
    }
}