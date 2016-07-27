using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UISmartObjectSpinnerList : UISmartObject
    {
        public UISmartObjectSpinnerList(SmartObject smartObject, ListData listData, BoolInputSig enableJoin, BoolInputSig visibleJoin)
            : base(smartObject, enableJoin, visibleJoin)
        {
            uint item = 1;
            this.Data = listData;
            if (this.Data != null)
                this.Data.DataChange += new ListDataChangeEventHandler(Data_DataChange);
            Buttons = new UISmartObjectButtonCollection();
            try
            {
                while (smartObject.BooleanOutput.Contains(string.Format("Item {0} Selected", item)))
                {
                    Buttons.Add(new UISmartObjectButton(this, item, this.DeviceSmartObject,
                        string.Format("Item {0} Selected", item),
                        null,
                        string.Format("Set Item {0} Text", item)));
                    item++;
                }

                this.MaxNumberOfItems = (ushort)(item - 1);
                this.NumberOfItems = 0;
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error constructing UISmartObjectSpinnerList: {0}", e.Message);
            }
        }
        
        protected ListData Data { get; set; }

        public ushort MaxNumberOfItems { get; protected set; }

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

        public ushort SelectedItem
        {
            get
            {
                return this.DeviceSmartObject.UShortOutput["Item Selected"].UShortValue;
            }
            set
            {
                if (value > 0)
                {
                    this.DeviceSmartObject.UShortInput["Select Item"].UShortValue = 0;
                    this.DeviceSmartObject.UShortInput["Select Item"].UShortValue = value;
                }
            }
        }

        private event SpinnerListSelectionChangedEventHandler _SelectionChanged;

        int subscribeCount = 0;

        public event SpinnerListSelectionChangedEventHandler SelectionChanged
        {
            add
            {
                if (subscribeCount == 0)
                    this.DeviceSmartObject.SigChange += new SmartObjectSigChangeEventHandler(DeviceSmartObject_SigChange);

                subscribeCount++;

                _SelectionChanged += value;
            }
            remove
            {
                subscribeCount--;

                if (subscribeCount == 0)
                    this.DeviceSmartObject.SigChange -= new SmartObjectSigChangeEventHandler(DeviceSmartObject_SigChange);
                
                _SelectionChanged -= value;
            }
        }

        void DeviceSmartObject_SigChange(GenericBase currentDevice, SmartObjectEventArgs args)
        {
            try
            {
                if (args.Event == eSigEvent.UShortChange
                    && args.Sig == this.DeviceSmartObject.UShortOutput["Item Selected"] && this.SelectedItem > 0)
                {
                    if (_SelectionChanged != null && this.Data != null)
                        _SelectionChanged(this, new SpinnerListSelectionChangedEventArgs(this.SelectedItem - 1, this.Data[this.SelectedItem - 1].DataObject));
                    else if (_SelectionChanged != null)
                        _SelectionChanged(this, new SpinnerListSelectionChangedEventArgs(this.SelectedItem - 1, null));
                }
            }
            catch (Exception e)
            {
                ErrorLog.Error("Error in UISmartObjectSpinnerList.DeviceSmartObject_SigChange(), {0}, {1} sig: {2}", e.Message, args.Event.ToString(), args.Sig.Name);
            }
        }

        private new UISmartObjectButtonCollection Buttons { set; get; }

        public UISmartObjectButton this[uint itemIndex]
        {
            get
            {
                return Buttons[itemIndex];
            }
        }

        protected virtual void Data_DataChange(ListData listData, ListDataChangeEventArgs args)
        {
            if (args.EventType == eListDataChangeEventType.HasCleared)
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
                    this.DeviceSmartObject.StringInput[string.Format("Set Item {0} Text", item)]
                        .StringValue = listData[(int)item - 1].Title;
                }
            }
            else if (args.EventType == eListDataChangeEventType.ItemSelectionHasChanged)
            {
                ListDataObject item = listData.Where(i => i.IsSelected).FirstOrDefault();
                if (item != null)
                {
                    int index = listData.IndexOf(item);
                    this.SelectedItem = (ushort)(index + 1);
                }
            }
        }
    }

    public delegate void SpinnerListSelectionChangedEventHandler(UISmartObjectSpinnerList list, SpinnerListSelectionChangedEventArgs args);

    public class SpinnerListSelectionChangedEventArgs : EventArgs
    {
        public SpinnerListSelectionChangedEventArgs(int dataObjectIndex, object linkedObject)
        {
            DataObjectIndex = dataObjectIndex;
            LinkedObject = linkedObject;
        }

        public int DataObjectIndex;
        public object LinkedObject;
    }
}