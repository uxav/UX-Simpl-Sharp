using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UISmartObject
    {
        public uint ID
        {
            get { return this.DeviceSmartObject.ID; }
        }
        protected SmartObject DeviceSmartObject;
        public UISmartObjectButtonCollection Buttons { get; protected set; }
        protected BoolInputSig EnableJoin { get; set; }
        protected BoolInputSig VisibleJoin { get; set; }
        private bool countedItems = false;
        private ushort _MaxNumberOfItems = 0;
        public virtual ushort MaxNumberOfItems
        {
            get
            {
                if (!countedItems)
                {
                    ushort item = 1;
                    while (this.DeviceSmartObject.BooleanInput.Any(s => s.Name.Contains(string.Format("Item {0} ", item))))
                        item++;
                    item--;
                    _MaxNumberOfItems = item;
                    countedItems = true;
                }
                return _MaxNumberOfItems;
            }
        }

        public virtual ushort NumberOfItems
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

        public UISmartObject(SmartObject smartObject)
        {
            this.Buttons = new UISmartObjectButtonCollection();
            this.DeviceSmartObject = smartObject;
        }

        public UISmartObject(SmartObject smartObject, BoolInputSig objectEnableJoin, BoolInputSig objectVisibleJoin)
        {
            this.Buttons = new UISmartObjectButtonCollection();
            this.DeviceSmartObject = smartObject;
            EnableJoin = objectEnableJoin;
            VisibleJoin = objectVisibleJoin;
        }

        public void AddButton(UISmartObjectButton button)
        {
            this.Buttons.Add(button);
        }

        public void AddButton(uint itemIndex, string digitalPressSigNam, string digitalFeedbackSigName)
        {
            if (this.DeviceSmartObject.BooleanOutput[digitalPressSigNam] != null)
            {
                UISmartObjectButton newButton = new UISmartObjectButton(this,
                    itemIndex, this.DeviceSmartObject, digitalPressSigNam, digitalFeedbackSigName
                    );
                this.Buttons.Add(newButton);
            }
        }

        public void AddButton(uint itemIndex, string digitalPressSigNam, string digitalFeedbackSigName,
            string titleFeedbackSigName, string iconFeedbackSigName)
        {
            if (this.DeviceSmartObject.BooleanOutput[digitalPressSigNam] != null)
            {
                UISmartObjectButton newButton = new UISmartObjectButton(this,
                    itemIndex, this.DeviceSmartObject, digitalPressSigNam, digitalFeedbackSigName,
                    titleFeedbackSigName, iconFeedbackSigName
                    );
                this.Buttons.Add(newButton);
            }
        }

        public void AddButton(uint itemIndex, string digitalPressSigNam, string digitalFeedbackSigName,
            string titleFeedbackSigName, string iconFeedbackSigName, string enableSigName, string visibleSigName)
        {
            if (this.DeviceSmartObject.BooleanOutput[digitalPressSigNam] != null)
            {
                UISmartObjectButton newButton = new UISmartObjectButton(this,
                    itemIndex, this.DeviceSmartObject, digitalPressSigNam, digitalFeedbackSigName,
                    titleFeedbackSigName, iconFeedbackSigName, enableSigName, visibleSigName
                    );
                this.Buttons.Add(newButton);
            }
        }

        public bool Enabled
        {
            set
            {
                if (this.EnableJoin != null)
                    this.EnableJoin.BoolValue = value;
            }
            get {
                if (this.EnableJoin != null)
                    return this.EnableJoin.BoolValue;
                return false;
            }
        }

        public bool Visible
        {
            set
            {
                if (this.VisibleJoin != null)
                    this.VisibleJoin.BoolValue = value;
            }
            get
            {
                if (this.VisibleJoin != null)
                    return this.VisibleJoin.BoolValue;
                return false;
            }
        }

        public void Show()
        {
            this.Visible = true;
        }

        public void Hide()
        {
            this.Visible = false;
        }

        public void Enable()
        {
            this.Enabled = true;
        }

        public void Disable()
        {
            this.Enabled = false;
        }

        private event UISmartObjectButtonEventHandler _ButtonEvent;

        int subscribeCount = 0;

        public event UISmartObjectButtonEventHandler ButtonEvent
        {
            add
            {
                if (subscribeCount == 0)
                    this.Buttons.ButtonEvent += new UISmartObjectButtonCollectionEventHandler(Buttons_ButtonEvent);

                subscribeCount++;

                _ButtonEvent += value;
            }
            remove
            {
                subscribeCount--;

                if (subscribeCount == 0)
                    this.Buttons.ButtonEvent -= new UISmartObjectButtonCollectionEventHandler(Buttons_ButtonEvent);

                _ButtonEvent -= value;
            }
        }

        protected void Buttons_ButtonEvent(UISmartObjectButtonCollection buttonCollection, UISmartObjectButtonCollectionEventArgs args)
        {
            if (this._ButtonEvent != null)
                this._ButtonEvent(this, new UISmartObjectButtonEventArgs(args.Button, args.EventType, args.HoldTime));
        }

        /// <summary>
        /// Unregister from any sig changes and dispose of resources
        /// </summary>
        public void Dispose()
        {
            // Dispose of unmanaged resources.
            Dispose(true);
            CrestronEnvironment.GC.SuppressFinalize(this);
        }

        bool disposed = false;

        public bool Disposed
        {
            get
            {
                return disposed;
            }
        }

        /// <summary>
        /// Override this to free resources
        /// </summary>
        /// <param name="disposing">true is Dispose() has been called</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here.
                //
            }

            // Free any unmanaged objects here.
            //
            if (subscribeCount > 0)
                this.Buttons.ButtonEvent -= new UISmartObjectButtonCollectionEventHandler(Buttons_ButtonEvent);
            this.Buttons.Dispose();

            disposed = true;
        }
    }

    public delegate void UISmartObjectButtonEventHandler(UISmartObject sObject, UISmartObjectButtonEventArgs args);

    public class UISmartObjectButtonEventArgs : EventArgs
    {
        public UIButtonEventType EventType;
        public uint ButtonIndex;
        public UISmartObjectButton Button;
        public long HoldTime;
        public UISmartObjectButtonEventArgs(UISmartObjectButton button, UIButtonEventType type, long holdTime)
            : base()
        {
            this.ButtonIndex = button.ItemIndex;
            this.Button = button;
            this.EventType = type;
            this.HoldTime = holdTime;
        }
    }
}