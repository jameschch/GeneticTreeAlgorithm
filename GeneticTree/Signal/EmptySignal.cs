using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GeneticTree.Signal
{
    public class EmptySignal : ISignal
    {

        public bool _isTrue = true;

        public EmptySignal(bool isTrue = true)
        {
            _isTrue = isTrue;
        }

        public ISignal Child { get; set; }

        public bool IsReady { get { return true; } }

        public Operator Operator { get; set; }

        public ISignal Sibling { get; set; }

        public bool IsTrue()
        {
            return _isTrue;
        }
    }
}
