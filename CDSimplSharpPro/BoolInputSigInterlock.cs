using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro
{
    public class BoolInputSigInterlock
    {
        private Dictionary<uint, BoolInputSig> Sigs;

        public BoolInputSigInterlock()
        {
            Sigs = new Dictionary<uint, BoolInputSig>();
        }

        public BoolInputSigInterlock(SigCollectionBase<BoolInputSig> sigCollection, uint startSigNumber, uint count)
        {
            Sigs = new Dictionary<uint, BoolInputSig>();

            for (uint n = startSigNumber; n <= startSigNumber + count - 1; n++)
            {
                Sigs.Add(n, sigCollection[n]);
            }
        }

        public void Add(BoolInputSig sig)
        {
            if (!this.Sigs.ContainsKey(sig.Number) && sig.Type == eSigType.Bool)
            {
                Sigs.Add(sig.Number, sig);
            }
        }

        public void Set(uint sigNumber)
        {
            foreach (BoolInputSig sig in Sigs.Values)
            {
                if (sig.BoolValue && sig.Number != sigNumber)
                {
                    sig.BoolValue = false;
                }
            }

            if (Sigs.ContainsKey(sigNumber))
            {
                Sigs[sigNumber].BoolValue = true;
            }
        }

        public void Set(BoolInputSig newSig)
        {
            foreach (BoolInputSig sig in Sigs.Values)
            {
                if (sig != newSig)
                {
                    sig.BoolValue = false;
                }

                if (Sigs.Values.Contains<BoolInputSig>(newSig))
                {
                    newSig.BoolValue = true;
                }
            }
        }

        public void ClearBool()
        {
            foreach (BoolInputSig sig in Sigs.Values)
            {
                sig.BoolValue = false;
            }
        }
    }
}