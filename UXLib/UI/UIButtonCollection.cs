using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using UXLib.Models;

namespace UXLib.UI
{
    public class UIButtonCollection : UXCollection<UIButton>, IDisposable
    {
        public UIButtonCollection() { }

        public override UIButton this[uint pressDigitalJoinNumber]
        {
            get
            {
                return base[pressDigitalJoinNumber];
            }
            internal set
            {
                base[pressDigitalJoinNumber] = value;
            }
        }

        public int NumberOfButtons
        {
            get
            {
                return this.Count;
            }
        }

        public void Add(UIButton button)
        {
            if (!this.Contains(button))
            {
                this[button.PressDigitalJoin.Number] = button;
                if (subscribeCount > 0)
                    button.ButtonEvent += new UIObjectButtonEventHandler(OnButtonEvent);
            }
        }

        protected virtual void OnButtonEvent(UIObject currentObject, UIObjectButtonEventArgs args)
        {
            if (this._ButtonEvent != null)
            {
                this._ButtonEvent(this, new UIButtonCollectionEventArgs(currentObject as UIButton, args.EventType, args.HoldTime, this.IndexOf(currentObject as UIButton)));
            }
        }

        private event UIButtonCollectionEventHandler _ButtonEvent;

        int subscribeCount = 0;

        public event UIButtonCollectionEventHandler ButtonEvent
        {
            add
            {
                if(subscribeCount == 0)
                    foreach (UIButton button in this)
                        button.ButtonEvent += new UIObjectButtonEventHandler(OnButtonEvent);
                
                subscribeCount++;

                _ButtonEvent += value;
            }
            remove
            {
                subscribeCount--;

                if (subscribeCount == 0)
                    foreach (UIButton button in this)
                        button.ButtonEvent -= new UIObjectButtonEventHandler(OnButtonEvent);

                _ButtonEvent -= value;
            }
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
            foreach (UIButton button in this)
            {
                if (subscribeCount > 0)
                    button.ButtonEvent -= new UIObjectButtonEventHandler(OnButtonEvent);
                button.Dispose();
            }

            InternalDictionary.Clear();

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