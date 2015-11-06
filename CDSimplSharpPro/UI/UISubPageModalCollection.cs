using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro.UI
{
    public class UISubPageModalCollection : IEnumerable<UISubPageModal>
    {
        private List<UISubPageModal> SubPages;
        BoolInputSigInterlock JoinGroup;

        public UISubPageModal this[uint joinNumber]
        {
            get
            {
                return this.SubPages.FirstOrDefault(p => p.VisibleJoinNumber == joinNumber);
            }
        }

        public UISubPageModal this[string keyName]
        {
            get
            {
                return this.SubPages.FirstOrDefault(p => p.KeyName == keyName);
            }
        }

        public UISubPageModalCollection()
        {
            this.SubPages = new List<UISubPageModal>();
            this.JoinGroup = new BoolInputSigInterlock();
        }

        public void Add(string key, BoolInputSig visibleJoinSig, UILabel titleLabel, string name, UITimeOut timeOut)
        {
            if (!this.JoinGroup.Contains(visibleJoinSig))
            {
                this.SubPages.Add(new UISubPageModal(key, visibleJoinSig, this.JoinGroup, titleLabel, name, timeOut));
            }
            else
            {
                throw new Exception("SubPage with visible join value already exists");
            }
        }

        public void Close()
        {
            foreach (UISubPageModal page in this.SubPages)
            {
                page.Hide();
            }
        }

        public IEnumerator<UISubPageModal> GetEnumerator()
        {
            return this.SubPages.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}