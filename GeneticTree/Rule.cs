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
        static Func<bool, bool, bool> not = (a, b) => a && !b;
        static Func<bool, bool, bool> orInclusive = (a, b) => (a || b);
        static Func<bool, bool, bool> nor = (a, b) => (a || !b);
        static Func<bool, bool, bool> norInclusive = (a, b) => (a || !b);

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
            return signal.IsReady && (signal.Sibling == null || IsReady(signal.Sibling));
        }

        public bool IsTrue()
        {
            return IsTrue(Signal);
        }

        public bool IsTrue(ISignal signal, bool? siblingIsTrue = null)
        {
            if (signal.Sibling == null)
            {
                return signal.IsTrue();
            }

            Func<bool, bool, bool> op = null;

            switch (signal.Operator)
            {
                case Operator.Or:
                    op = or;
                    break;
                case Operator.OrInclusive:
                    op = orInclusive;
                    break;
                case Operator.Not:
                    op = not;
                    break;
                case Operator.Nor:
                    op = nor;
                    break;
                case Operator.NorInclusive:
                    op = norInclusive;
                    break;
                default:
                    op = and;
                    break;
            };

            var next = signal.Sibling;

            return op(signal.IsTrue(), IsTrue(next));
        }

        public void Update(BaseData data)
        {
            Signal.Update(data);
        }

    }
}