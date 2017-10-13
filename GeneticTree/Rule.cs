using GeneticTree.Signal;
using QuantConnect.Data;
using QuantConnect.Indicators;
using System;
using System.Linq;
using System.Text;

namespace GeneticTree
{

    public class Rule
    {

        static Func<bool, bool, bool> and = (a, b) => a && b;
        static Func<bool, bool, bool> or = (a, b) => a || b;
        static Func<bool, bool, bool> andFirst = (a, b) => (a && b);
        static Func<bool, bool, bool> orFirst = (a, b) => (a || b);

        public ISignal Signal { get; }

        public Rule(ISignal signal)
        {
            Signal = signal;
        }

        public bool IsReady()
        {
            return IsReady(Signal);
        }

        private bool IsReady(ISignal signal)
        {
            return signal.IsReady && ((signal.Child == null && signal.Sibling == null) 
                || ((signal.Child == null || IsReady(signal.Child)) && (signal.Sibling == null || IsReady(signal.Sibling))));
        }

        public bool IsTrue()
        {
            return IsTrue(Signal);
        }

        public bool IsTrue(ISignal signal, bool? siblingIsTrue = null)
        {

            if (signal.Child == null && signal.Sibling == null)
            {
                return signal.IsTrue();
            }

            bool isSibling = signal.Sibling != null;

            var op = signal.Operator == Operator.AND ? isSibling ? andFirst : and : isSibling ? orFirst : or;

            var next = isSibling ? signal.Sibling : signal.Child;

            return op(signal.IsTrue(), IsTrue(next));
        }

        public void Update(BaseData data)
        {
            Signal.Update(data);
        }

    }
}