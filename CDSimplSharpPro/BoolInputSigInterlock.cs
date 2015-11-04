using System;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using Crestron.SimplSharp;
using Crestron.SimplSharpPro;

namespace CDSimplSharpPro
{
    public class BoolInputSigInterlock : IEnumerable<BoolInputSig>
    {
        private List<BoolInputSig> Sigs;

        public BoolInputSig CurrentSig
        {
            get
            {
                return Sigs.FirstOrDefault(s => s.BoolValue == true);
            }
        }

        public BoolInputSig this[int index]
        {
            get
            {
                return this.Sigs[index];
            }
        }

        public BoolInputSigInterlock()
        {
            Sigs = new List<BoolInputSig>();
        }

        public BoolInputSigInterlock(SigCollectionBase<BoolInputSig> sigCollection, uint startSigNumber, uint count)
        {
            Sigs = new List<BoolInputSig>();

            for (uint n = startSigNumber; n <= startSigNumber + count - 1; n++)
            {
                Sigs.Add(sigCollection[n]);
            }
        }

        public void Add(BoolInputSig sig)
        {
            if (!this.Sigs.Contains(sig) && sig.Type == eSigType.Bool)
            {
                Sigs.Add(sig);
            }
        }

        public void Set(uint sigNumber)
        {
            foreach (BoolInputSig sig in Sigs)
            {
                if (sig.BoolValue && sig.Number != sigNumber)
                {
                    sig.BoolValue = false;
                }
            }

            BoolInputSig newSig = Sigs.FirstOrDefault(s => s.Number == sigNumber);

            if (newSig != null)
            {
                newSig.BoolValue = true;
            }
        }

        public void Set(BoolInputSig newSig)
        {
            foreach (BoolInputSig sig in Sigs)
            {
                if (sig != newSig)
                {
                    sig.BoolValue = false;
                }

                if (Sigs.Contains(newSig))
                {
                    newSig.BoolValue = true;
                }
            }
        }

        public void ClearBool()
        {
            foreach (BoolInputSig sig in Sigs)
            {
                sig.BoolValue = false;
            }
        }

        public IEnumerator<BoolInputSig> GetEnumerator()
        {
            return this.Sigs.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}