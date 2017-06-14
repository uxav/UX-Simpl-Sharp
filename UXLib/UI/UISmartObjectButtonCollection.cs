using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace UXLib.UI
{
    public class UISmartObjectButtonCollection : IEnumerable<UISmartObjectButton>
    {
        public UISmartObjectButton this[uint itemIndex]
        {
            get
            {
                return this.Buttons.FirstOrDefault(b => b.ItemIndex == itemIndex);
            }
        }
        
        private List<UISmartObjectButton> Buttons;

        public int NumberOfButtons
        {
            get
            {
                return this.Buttons.Count;
            }
        }

        public UISmartObjectButtonCollection()
        {
            this.Buttons = new List<UISmartObjectButton>();
        }

        public void Add(UISmartObjectButton button)
        {
            if (!this.Buttons.Contains(button))
            {
                this.Buttons.Add(button);
                if (subscribeCount > 0)
                    button.ButtonEvent += new UIObjectButtonEventHandler(OnButtonEvent);
            }
        }

        protected virtual void OnButtonEvent(UIObject currentObject, UIObjectButtonEventArgs args)
        {
            if (this._ButtonEvent != null)
            {
                this._ButtonEvent(this, new UISmartObjectButtonCollectionEventArgs(currentObject as UISmartObjectButton, args.EventType, args.HoldTime));
            }
        }

        public IEnumerator<UISmartObjectButton> GetEnumerator()
        {
            return Buttons.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private event UISmartObjectButtonCollectionEventHandler _ButtonEvent;

        int subscribeCount = 0;

        public event UISmartObjectButtonCollectionEventHandler ButtonEvent
        {
            add
            {
                if(subscribeCount == 0)
                    foreach(UISmartObjectButton button in this.Buttons)
                        button.ButtonEvent += new UIObjectButtonEventHandler(OnButtonEvent);

                subscribeCount++;

                _ButtonEvent += value;
            }
            remove
            {
                subscribeCount--;

                if (subscribeCount == 0)
                    foreach (UISmartObjectButton button in this.Buttons)
                        button.ButtonEvent -= new UIObjectButtonEventHandler(OnButtonEvent);

                _ButtonEvent -= value;
            }
        }

        public UISmartObjectButton UISmartObjectButtonBySigNumber(uint pressDigitalJoinNumber)
        {
            return this.Buttons.FirstOrDefault(b => b.PressDigitalJoin.Number == pressDigitalJoinNumber);
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
            foreach (UISmartObjectButton button in Buttons)
            {
                if (subscribeCount > 0)
                    button.ButtonEvent -= new UIObjectButtonEventHandler(OnButtonEvent);
                button.Dispose();
            }

            Buttons.Clear();
            Buttons = null;

            disposed = true;
        }
    }

    public delegate void UISmartObjectButtonCollectionEventHandler(UISmartObjectButtonCollection buttonCollection, UISmartObjectButtonCollectionEventArgs args);

    public class UISmartObjectButtonCollectionEventArgs : EventArgs
    {
        public UIButtonEventType EventType;
        public UISmartObjectButton Button;
        public long HoldTime;
        public UISmartObjectButtonCollectionEventArgs(UISmartObjectButton button, UIButtonEventType type, long holdTime)
            : base()
        {
            this.Button = button;
            this.EventType = type;
            this.HoldTime = holdTime;
        }
    }
}