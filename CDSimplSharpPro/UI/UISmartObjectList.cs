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
        private ListData Data;
        public ushort MaxNumberOfItems { get; private set; }
        private BoolInputSig LoadingSubPageOverlay;

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

        public bool IsMoving
        {
            get
            {
                if (this.DeviceSmartObject.BooleanOutput["Is Moving"] != null)
                    return this.DeviceSmartObject.BooleanOutput["Is Moving"].BoolValue;
                return false;
            }
        }

        public UISmartObjectList(UIKey key, SmartObject smartObject, ListData listData, BoolInputSig enableJoin, BoolInputSig visibleJoin)
            : base(key, smartObject, enableJoin, visibleJoin)
        {
            uint item = 1;
            this.Data = listData;
            this.Data.DataChange += new ListDataChangeEventHandler(Data_DataChange);
            this.DeviceSmartObject.SigChange +=new SmartObjectSigChangeEventHandler(DeviceSmartObject_SigChange);
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

        void Data_DataChange(ListData listData, ListDataChangeEventArgs args)
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
        }

        public object LinkedObjectForButton(uint buttonIndex)
        {
            return this.Buttons[buttonIndex].LinkedObject;
        }

        void DeviceSmartObject_SigChange(GenericBase currentDevice, SmartObjectEventArgs args)
        {
            switch (args.Sig.Type)
            {
                case eSigType.Bool:
                    {
                        if (args.Sig.Name == "Is Moving")
                        {
                            //
                        }
                        break;
                    }
            }
        }

        public void LoadingSubPageOverlayAssign(BoolInputSig loadingSubPageOverlaySig)
        {
            this.LoadingSubPageOverlay = loadingSubPageOverlaySig;
        }
    }
}