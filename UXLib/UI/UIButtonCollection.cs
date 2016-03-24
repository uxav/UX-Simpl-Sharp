using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;

namespace UXLib.UI
{
    public class UIButtonCollection : IEnumerable<UIButton>
    {
        private List<UIButton> Buttons;

        public UIButton this[uint pressDigitalJoinNumber]
        {
            get
            {
                return this.Buttons.FirstOrDefault(b => b.PressDigitalJoin.Number == pressDigitalJoinNumber);
            }
        }

        public int NumberOfButtons
        {
            get
            {
                return this.Buttons.Count;
            }
        }

        public UIButtonCollection()
        {
            this.Buttons = new List<UIButton>();
        }

        public void Add(UIButton button)
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
                this._ButtonEvent(this, new UIButtonCollectionEventArgs(currentObject as UIButton, args.EventType, args.HoldTime, Buttons.IndexOf(currentObject as UIButton)));
            }
        }

        public IEnumerator<UIButton> GetEnumerator()
        {
            return Buttons.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        private event UIButtonCollectionEventHandler _ButtonEvent;

        int subscribeCount = 0;

        public event UIButtonCollectionEventHandler ButtonEvent
        {
            add
            {
                if(subscribeCount == 0)
                    foreach (UIButton button in Buttons)
                        button.ButtonEvent += new UIObjectButtonEventHandler(OnButtonEvent);
                
                subscribeCount++;

                _ButtonEvent += value;
            }
            remove
            {
                subscribeCount--;

                if (subscribeCount == 0)
                    foreach (UIButton button in Buttons)
                        button.ButtonEvent -= new UIObjectButtonEventHandler(OnButtonEvent);

                _ButtonEvent -= value;
            }
        }

        public int IndexOf(UIButton button)
        {
            return Buttons.IndexOf(button);
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
            foreach (UIButton button in Buttons)
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

    public delegate void UIButtonCollectionEventHandler(UIButtonCollection buttonCollection, UIButtonCollectionEventArgs args);

    public class UIButtonCollectionEventArgs : EventArgs
    {
        public UIButtonEventType EventType;
        public UIButton Button;
        public long HoldTime;
        public int ButtonIndexInCollection;
        public UIButtonCollectionEventArgs(UIButton button, UIButtonEventType type, long holdTime, int buttonIndex)
            : base()
        {
            this.Button = button;
            this.EventType = type;
            this.HoldTime = holdTime;
            this.ButtonIndexInCollection = buttonIndex;
        }
    }
}