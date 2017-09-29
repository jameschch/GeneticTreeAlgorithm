using GeneticTree.Signal;
using System;
using System.Linq;
using System.Text;

namespace GeneticTree
{

    public class Rule
    {
        private readonly ISignal _signal;
        static Func<bool, bool, bool> and = (a, b) => a && b;
        static Func<bool, bool, bool> or = (a, b) => a || b;
        static Func<bool, bool, bool> andFirst = (a, b) => (a && b);
        static Func<bool, bool, bool> orFirst = (a, b) => (a || b);

        public Rule(ISignal signal)
        {
            _signal = signal;
        }

        public bool IsReady()
        {
            return IsReady(_signal);
        }

        private bool IsReady(ISignal signal)
        {
            return signal.IsReady && signal.Child == null || IsReady(signal.Child);
        }

        public bool IsTrue()
        {
            return IsTrue(_signal);
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

    }
}