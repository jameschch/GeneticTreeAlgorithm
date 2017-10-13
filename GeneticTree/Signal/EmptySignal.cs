using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuantConnect.Data;
using QuantConnect.Indicators;

namespace GeneticTree.Signal
{
    public class EmptySignal : ISignal
    {

        public bool _isTrue = true;

        public EmptySignal(bool isTrue = true)
        {
            _isTrue = isTrue;
        }

        public bool IsReady { get { return true; } }

        public Operator Operator { get; set; }

        public ISignal Sibling { get; set; }

        public bool IsTrue()
        {
            return _isTrue;
        }

        public void Update(BaseData data)
        {
        }
    }
}
