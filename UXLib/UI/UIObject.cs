using System;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;
using Crestron.SimplSharpPro.DeviceSupport;
using Crestron.SimplSharpPro.CrestronThread;

namespace UXLib.UI
{
    public abstract class UIObject : IDisposable, IUIVisibleObject
    {
        /// <summary>
        /// The Digital output join which comes from a press
        /// </summary>
        public BoolOutputSig PressDigitalJoin { get; protected set; }

        /// <summary>
        /// The Feedback input digital join going back to a button feedback status
        /// </summary>
        public BoolInputSig FeedbackDigitalJoin { get; protected set; }

        /// <summary>
        /// A digital output join which goes high once a view has completed a transition
        /// </summary>
        public BoolOutputSig TransitionCompleteDigitalJoin { get; protected set; }
        
        /// <summary>
        /// Digital input join which enables an object
        /// </summary>
        public BoolInputSig EnableDigitalJoin { get; protected set; }

        /// <summary>
        /// Digital input join which makes the object visible or not
        /// </summary>
        public BoolInputSig VisibleDigitalJoin { get; protected set; }

        /// <summary>
        /// Digital output join usually used on a page to show it's become visible
        /// </summary>
        public BoolOutputSig VisibleFeedbackJoin { get; protected set; }

        /// <summary>
        /// Text serial input join to set the indirect text on an object
        /// </summary>
        public StringInputSig TextSerialJoin { get; protected set; }

        /// <summary>
        /// Analog input join to set the multi-state mode of an object
        /// </summary>
        public UShortInputSig AnalogModeJoin { get; protected set; }
        
        /// <summary>
        /// Analog feedback input join which can set the level on an object
        /// </summary>
        public UShortInputSig AnalogFeedbackJoin { get; protected set; }

        /// <summary>
        /// Analog output join from device to give the current value
        /// Usually used on a slider for touch position / level
        /// </summary>
        public UShortOutputSig AnalogTouchJoin { get; protected set; }

        /// <summary>
        /// Use to set an icon by name
        /// </summary>
        public StringInputSig IconSerialJoin { get; protected set; }

        /// <summary>
        /// Gets the device which owns this UIObject
        /// </summary>
        public BasicTriList Device
        {
            get
            {
                try
                {
                    if (PressDigitalJoin != null)
                        return PressDigitalJoin.Owner as BasicTriList;
                    if (FeedbackDigitalJoin != null)
                        return FeedbackDigitalJoin.Owner as BasicTriList;
                    if (TextSerialJoin != null)
                        return TextSerialJoin.Owner as BasicTriList;
                    if (VisibleDigitalJoin != null)
                        return VisibleDigitalJoin.Owner as BasicTriList;
                    if (EnableDigitalJoin != null)
                        return EnableDigitalJoin.Owner as BasicTriList;
                    if (AnalogFeedbackJoin != null)
                        return AnalogFeedbackJoin.Owner as BasicTriList;
                    if (AnalogTouchJoin != null)
                        return AnalogTouchJoin.Owner as BasicTriList;
                    if (AnalogModeJoin != null)
                        return AnalogModeJoin.Owner as BasicTriList;
                    if (TransitionCompleteDigitalJoin != null)
                        return TransitionCompleteDigitalJoin.Owner as BasicTriList;
                    return null;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// Set the visibility of the object
        /// </summary>
        public virtual bool Visible
        {
            get
            {
                return VisibleDigitalJoin == null || VisibleDigitalJoin.BoolValue;
            }
            set
            {
                if (VisibleDigitalJoin == null) return;
#if DEBUG
                CrestronConsole.PrintLine("UIObject.set_Visible {0} -> {1} (Digital join {2})",
                    VisibleDigitalJoin.BoolValue, value, VisibleDigitalJoin.Number);
#endif
                VisibleDigitalJoin.BoolValue = value;
#if DEBUG
                CrestronConsole.PrintLine(VisibleDigitalJoin.ToString());
#endif
                if (value) OnShow();
                else OnHide();
            }
        }

        /// <summary>
        /// Called once the object has been shown / become visible
        /// </summary>
        protected virtual void OnShow()
        {

        }

        /// <summary>
        /// Called once the object has been hidden / not visible
        /// </summary>
        protected virtual void OnHide()
        {

        }

        public virtual bool Feedback
        {
            get
            {
                if (this.FeedbackDigitalJoin != null)
                    return this.FeedbackDigitalJoin.BoolValue;
                return false;
            }
            set
            {
                if (this.FeedbackDigitalJoin != null)
                    this.FeedbackDigitalJoin.BoolValue = value;
            }
        }

        /// <summary>
        /// Set the enable / disable state of the object
        /// </summary>
        public virtual bool Enabled
        {
            get
            {
                if (this.EnableDigitalJoin != null)
                    return this.EnableDigitalJoin.BoolValue;
                return true;
            }
            set
            {
                if (this.EnableDigitalJoin != null)
                    this.EnableDigitalJoin.BoolValue = value;
            }
        }

        /// <summary>
        /// Set the indirect text join of the object
        /// </summary>
        public virtual string Text
        {
            get
            {
                if (this.TextSerialJoin != null)
                    return this.TextSerialJoin.StringValue;
                return string.Empty;
            }
            set
            {
                if (this.TextSerialJoin != null)
                    this.TextSerialJoin.StringValue = value;
            }
        }

        /// <summary>
        /// Set the analog feedback join of the object
        /// </summary>
        public virtual ushort AnalogFeedbackValue
        {
            get
            {
                return this.AnalogFeedbackJoin.UShortValue;
            }
            set
            {
                this.AnalogFeedbackJoin.UShortValue = value;
            }
        }

        public bool SupportsModeStates
        {
            get
            {
                if (this.AnalogModeJoin != null)
                    return true;
                return false;
            }
        }

        /// <summary>
        /// Set the the state / mode of an object
        /// </summary>
        public virtual ushort Mode
        {
            get
            {
                if (this.AnalogModeJoin != null)
                    return this.AnalogModeJoin.UShortValue;
                return 0;
            }
            set
            {
                if (this.AnalogModeJoin != null)
                    this.AnalogModeJoin.UShortValue = value;
            }
        }

        /// <summary>
        /// Set the object to use an icon by reference name
        /// </summary>
        public virtual string Icon
        {
            set
            {
                if (this.IconSerialJoin != null)
                    this.IconSerialJoin.StringValue = value;
            }
            get
            {
                if (this.IconSerialJoin != null)
                    return this.IconSerialJoin.StringValue;
                return string.Empty;
            }
        }

        CTimer holdTimer;

        long _holdTime;
        bool holdTimeSet = false;

        /// <summary>
        /// Set or get the hold time for a button to call a hold event
        /// </summary>
        public long HoldTime
        {
            get
            {
                if (!holdTimeSet)
                    return 500;
                else
                    return _holdTime;
            }
            set
            {
                holdTimeSet = true;
                _holdTime = value;
            }
        }

        /// <summary>
        /// The current time the button has been held for
        /// </summary>
        public long CurrentHoldTime { get; private set; }

        void holdTimerUpdate(object obj)
        {
            this.CurrentHoldTime = this.CurrentHoldTime + 100;

            if (this.CurrentHoldTime == this.HoldTime && _buttonEvent != null)
                _buttonEvent(this,
                    new UIObjectButtonEventArgs(
                        UIButtonEventType.Held, this.CurrentHoldTime));
        }

        private bool _pressState;

        /// <summary>
        /// Called once a button has been pressed and invokes any notifications
        /// and starts timers for holds etc
        /// </summary>
        protected virtual void OnPress()
        {
            _pressState = true;

            if (HoldTime > 0 && holdTimer == null || holdTimer.Disposed)
            {
                this.CurrentHoldTime = 0;
                holdTimer = new CTimer(holdTimerUpdate, null, 100, 100);
            }

#if DEBUG
            CrestronConsole.PrintLine("OnPress() - {0}, Digital Join {1}", this.GetType().ToString(),
                this.PressDigitalJoin.Number);
#endif

            if (_buttonEvent != null)
                _buttonEvent(this,
                    new UIObjectButtonEventArgs(
                        UIButtonEventType.Pressed, this.CurrentHoldTime));
        }

        /// <summary>
        /// Called once the button has been released and invokes any notifications
        /// and stops the timers
        /// </summary>
        protected virtual void OnRelease()
        {
            _pressState = false;

            if (holdTimer != null)
            {
                holdTimer.Stop();
                holdTimer.Dispose();
            }

            if (_buttonEvent != null)
                _buttonEvent(this,
                    new UIObjectButtonEventArgs(
                        UIButtonEventType.Released, this.CurrentHoldTime));

            if (this.CurrentHoldTime < this.HoldTime && _buttonEvent != null)
                _buttonEvent(this,
                    new UIObjectButtonEventArgs(
                        UIButtonEventType.Tapped, this.CurrentHoldTime));
        }

        /// <summary>
        /// Simulate a push and release event on the button
        /// </summary>
        public virtual void DoPush()
        {
            OnPress();
            OnRelease();
        }

        /// <summary>
        /// Simulate a push and release event on the button
        /// </summary>
        /// <param name="push">True - Trigger OnPush(), False - Trigger OnRelease()</param>
        /// <returns></returns>
        public virtual void DoPush(bool push)
        {
            if (push) OnPress();
            else OnRelease();
        }

        /// <summary>
        /// Called once a value from an analog touch join has been changed
        /// Will send any subscribed notifications
        /// </summary>
        /// <param name="newValue">The new value of the join</param>
        protected virtual void OnValueChange(ushort newValue)
        {
            if (this._valueChangeEvent != null && this.PressDigitalJoin != null)
                this._valueChangeEvent(this, new UIObjectAnalogTouchEventArgs(newValue,
                    this.AnalogTouchJoin.IsRamping,
                    this.PressDigitalJoin.BoolValue));
            else if (this._valueChangeEvent != null)
                this._valueChangeEvent(this, new UIObjectAnalogTouchEventArgs(newValue,
                    this.AnalogTouchJoin.IsRamping));
        }

        /// <summary>
        /// Called if the transition complete join goes high
        /// </summary>
        protected virtual void OnTransitionComplete()
        {

        }

        bool subscribed = false;
        public SmartObject SmartObject { get; set; }
        
        /// <summary>
        /// Use this to set a sig change event handler on the device
        /// </summary>
        protected void SubscribeToSigChanges()
        {
            if (!subscribed)
            {
#if DEBUG
                CrestronConsole.PrintLine("{0}.SubscribeToSigChanges()", GetType().Name);
#endif
                if(this.SmartObject != null)
                    this.SmartObject.SigChange += new SmartObjectSigChangeEventHandler(OnSigChange);
                else
                    this.Device.SigChange += new SigEventHandler(OnSigChange);
                subscribed = true;
            }
        }

        /// <summary>
        /// Use this to set a sig change event handler on the device
        /// </summary>
        protected void UnSubscribeToSigChanges()
        {
            if (subscribed)
            {
                if(_pressState)
                    OnRelease();

                if(this.SmartObject != null)
                    this.SmartObject.SigChange -= new SmartObjectSigChangeEventHandler(OnSigChange);
                else
                    this.Device.SigChange -= new SigEventHandler(OnSigChange);
                subscribed = false;
            }
        }

        /// <summary>
        /// Called once a sig change occurs on a device
        /// </summary>
        /// <param name="currentDevice">This device</param>
        /// <param name="args">The SigEventArgs</param>
        protected virtual void OnSigChange(GenericBase currentDevice, SigEventArgs args)
        {
            if (args.Event == eSigEvent.BoolChange && this.PressDigitalJoin != null && args.Sig == this.PressDigitalJoin)
            {
                if (args.Sig.BoolValue)
                    OnPress();
                else
                    OnRelease();
            }
            else if (args.Event == eSigEvent.BoolChange && this.VisibleFeedbackJoin != null && args.Sig == this.VisibleFeedbackJoin)
            {
#if DEBUG
                CrestronConsole.PrintLine("{0}.OnSigChange - VisibleFeedbackJoin {1} = {2}",
                    GetType().Name, VisibleFeedbackJoin.Number, VisibleFeedbackJoin.BoolValue);
#endif
                Visible = args.Sig.BoolValue;
            }
            else if (args.Event == eSigEvent.BoolChange && this.TransitionCompleteDigitalJoin != null && args.Sig == this.TransitionCompleteDigitalJoin)
            {
                if (args.Sig.BoolValue)
                    OnTransitionComplete();
            }
            else if (args.Event == eSigEvent.UShortChange && this.AnalogTouchJoin != null && args.Sig == this.AnalogTouchJoin)
            {
                OnValueChange(args.Sig.UShortValue);

                new Thread(ValueRampThread, args.Sig);
            }
        }

        object ValueRampThread(object sigObject)
        {
            UShortOutputSig sig = sigObject as UShortOutputSig;
            while (sig.IsRamping)
            {
                OnValueChange(sig.UShortValue);
                Thread.Sleep(10);
            }
            return null;
        }

        private event UIObjectButtonEventHandler _buttonEvent;

        int subscribeCount = 0;

        /// <summary>
        /// Called once a digital press join event is triggered
        /// </summary>
        public event UIObjectButtonEventHandler ButtonEvent
        {
            add
            {
                subscribeCount++;
                if (!subscribed)
                    SubscribeToSigChanges();
                _buttonEvent += value;
            }
            remove
            {
                subscribeCount--;
                if (subscribeCount == 0)
                    UnSubscribeToSigChanges();
                _buttonEvent -= value;
            }
        }

        private event UIObjectAnalogTouchEventHandler _valueChangeEvent;

        /// <summary>
        /// Called once a value is changed from an analog touch join
        /// </summary>
        public event UIObjectAnalogTouchEventHandler ValueChangeEvent
        {
            add
            {
                subscribeCount++;
                if (!subscribed)
                    SubscribeToSigChanges();
                _valueChangeEvent += value;
            }
            remove
            {
                subscribeCount--;
                if (subscribeCount == 0)
                    UnSubscribeToSigChanges();
                _valueChangeEvent -= value;
            }
        }

        /// <summary>
        /// Set the visible state as true
        /// </summary>
        public virtual void Show()
        {
#if DEBUG
            CrestronConsole.PrintLine("UIObject.Show()");
#endif
            this.Visible = true;
        }

        /// <summary>
        /// Set the visible state as false
        /// </summary>
        public virtual void Hide()
        {
            this.Visible = false;
        }

        /// <summary>
        /// Set the Enable join as true
        /// </summary>
        public virtual void Enable()
        {
            this.Enabled = true;
        }

        /// <summary>
        /// Set the Enable join as false
        /// </summary>
        public virtual void Disable()
        {
            this.Enabled = false;
        }
        
        /// <summary>
        /// Unregister from any sig changes and dispose of resources
        /// </summary>
        public void Dispose()
        {
            if (!Disposed)
            {
                // Dispose of unmanaged resources.
                Dispose(true);
                CrestronEnvironment.GC.SuppressFinalize(this);
            }
        }

        bool _Disposed = false;

        public bool Disposed
        {
            get
            {
                return _Disposed;
            }
            protected set
            {
                _Disposed = value;
            }
        }

        /// <summary>
        /// Override this to free resources
        /// </summary>
        /// <param name="disposing">true is Dispose() has been called</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                UnSubscribeToSigChanges();
                
                this.AnalogFeedbackJoin = null;
                this.AnalogModeJoin = null;
                this.AnalogTouchJoin = null;
                this.EnableDigitalJoin = null;
                this.FeedbackDigitalJoin = null;
                if (this.holdTimer != null)
                    this.holdTimer.Dispose();
                this.IconSerialJoin = null;
                this.PressDigitalJoin = null;
                this.SmartObject = null;
                this.TextSerialJoin = null;
                this.TransitionCompleteDigitalJoin = null;
                this.VisibleDigitalJoin = null;
                this.VisibleFeedbackJoin = null;
            }
            
            Disposed = true;
        }
    }

    public delegate void UIObjectButtonEventHandler(UIObject currentObject, UIObjectButtonEventArgs args);

    public class UIObjectButtonEventArgs : EventArgs
    {
        public UIObjectButtonEventArgs(UIButtonEventType eventType, long holdTime)
        {
            this.HoldTime = holdTime;
            this.EventType = eventType;
        }

        public long HoldTime { get; protected set; }
        public UIButtonEventType EventType { get; protected set; }
    }

    /// <summary>
    /// Press event type from digital join
    /// </summary>
    public enum UIButtonEventType
    {
        Pressed,
        Released,
        Held,
        Tapped
    }

    public delegate void UIObjectAnalogTouchEventHandler(UIObject currentObject, UIObjectAnalogTouchEventArgs args);

    public class UIObjectAnalogTouchEventArgs : EventArgs
    {
        public UIObjectAnalogTouchEventArgs(ushort newValue, bool isRamping)
        {
            this.NewValue = newValue;
            this.IsRamping = isRamping;
        }

        public UIObjectAnalogTouchEventArgs(ushort newValue, bool isRamping, bool isBeingPressed)
            : this(newValue, isRamping)
        {
            this.IsBeingPressed = isBeingPressed;
        }

        public ushort NewValue { get; protected set; }
        public bool IsBeingPressed { get; protected set; }
        public bool IsRamping { get; protected set; }
    }
}