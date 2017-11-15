using System;
using Crestron.SimplSharp;

namespace UXLib.UI
{
    public class UIActionSheet : IDisposable
    {
        public UIActionSheet(UISubPage subPage, string title, string subTitle, Action<UIActionSheet, ActionSheetButtonAction> callBack)
        {
            this.SubPage = subPage;
            this.SubPage.Title = title;
            this.SubPage.SubTitle = subTitle;
            this.Buttons = new UIButtonCollection();
            this.CallBack = callBack;
        }

        public UISubPage SubPage { get; protected set; }
        public UIButtonCollection Buttons { get; protected set; }
        Action<UIActionSheet, ActionSheetButtonAction> CallBack;

        public virtual void AddButton(UIActionSheetButton button)
        {
            this.Buttons.Add(button);
        }

        public virtual void Show()
        {
            action = ActionSheetButtonAction.TimedOut;
            if (!this.Visible)
            {
                this.SubPage.VisibilityChange += new UIViewBaseVisibitlityEventHandler(SubPage_VisibilityChange);
                this.SubPage.Show();
            }
        }

        public virtual void Show(bool withTimeout)
        {
            Show();
            if (!withTimeout)
                this.SubPage.TimeOut.Cancel();
        }

        public virtual void Hide()
        {
            this.SubPage.Hide();
        }

        public bool Visible
        {
            get
            {
                return this.SubPage.Visible;
            }
        }

        public string Title
        {
            get { return this.SubPage.Title; }
            set { this.SubPage.Title = value; }
        }

        public string SubTitle
        {
            get { return this.SubPage.SubTitle; }
            set { this.SubPage.SubTitle = value; }
        }

        ActionSheetButtonAction action;

        void SubPage_VisibilityChange(UIViewBase sender, UIViewVisibilityEventArgs args)
        {
#if DEBUG
            CrestronConsole.PrintLine("{0} SubPage Visibility Changed: {1}", this.GetType().ToString(), args.EventType.ToString());
#endif
            if (args.EventType == eViewEventType.WillShow)
            {
                this.Buttons.ButtonEvent += new UIButtonCollectionEventHandler(Buttons_ButtonEvent);
            }
            else if (args.EventType == eViewEventType.DidHide)
            {
                this.SubPage.VisibilityChange -= new UIViewBaseVisibitlityEventHandler(SubPage_VisibilityChange);
                this.Buttons.ButtonEvent -= new UIButtonCollectionEventHandler(Buttons_ButtonEvent);
                if (this.CallBack != null)
                    this.CallBack(this, action);
            }
        }

        void Buttons_ButtonEvent(UIButtonCollection group, UIButtonCollectionEventArgs args)
        {
            if (args.EventType == UIButtonEventType.Released)
            {
                UIActionSheetButton responseButton = args.Button as UIActionSheetButton;
                action = responseButton.Action;
                this.Hide();
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
        /// Unregister from any sig changes and dispose of resources
        /// </summary>
        public void Dispose()
        {
#if DEBUG
            CrestronConsole.PrintLine("{0} Dispose() Called!", this.GetType().ToString());
#endif
            // Dispose of unmanaged resources.
            Dispose(true);
            CrestronEnvironment.GC.SuppressFinalize(this);
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
                if (this.Visible)
                    this.SubPage.Hide();
                foreach (UIButton button in Buttons)
                {
                    button.Dispose();
                }
                this.SubPage.Dispose();
            }
        }
    }
}