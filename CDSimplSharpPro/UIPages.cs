using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro
{
    public class UIPages<TKey> : Dictionary<TKey, UIPage>
    {
        BoolInputSigInterlock PageJoinSigGroup;

        public UIPages()
            : base()
        {
            this.PageJoinSigGroup = new BoolInputSigInterlock();
        }

        public void Add(TKey key, string name, BoolInputSig pageJoinSig)
        {
            if (!this.ContainsKey(key))
            {
                UIPage newPage = new UIPage(name, pageJoinSig, this.PageJoinSigGroup);
                this.Add(key, newPage);
            }
            else
            {
                throw new Exception("Page with key value already exists");
            }
        }
    }
}