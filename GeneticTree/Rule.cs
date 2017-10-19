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

        public ISignal[] Signal { get; }

        public Rule(ISignal[] signal)
        {
            Signal = signal;
        }

        public bool IsReady()
        {
            return Signal.All(s => s.IsReady);
        }

        public bool IsTrue()
        {
            foreach (var item in Signal)
            {
                Func<bool, bool, bool> op = null;

                string expression;

                switch (item.Operator)
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

                bool isTrue = item.IsTrue();
                bool nextTrue = item.Sibling != null ? item.Sibling.IsTrue() : true;

                if (new [] { and, or, not }.Contains(op))
                {

                }

            }


            var next = signal.Sibling;

            return op(signal.IsTrue(), IsTrue(next));
        }

        public void Update(BaseData data)
        {
            Signal.Update(data);
        }

    }
}