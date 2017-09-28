using GeneticTree.Signal;
using System;
using System.Linq;
using System.Text;

namespace GeneticTree
{
    /// <summary>
    ///     This class wires and evaluates dynamically a set of <see cref="ISignal" /> and a set of logical
    ///     operators in string format in the form:
    ///     Indicator1|Operator1|Indicator2|Operator2|...|IndicatorN|OperatorN|
    /// </summary>
    public class Rule
    {
        private readonly ISignal _signal;
        static Func<bool, bool, bool> and = (a, b) => a && b;
        static Func<bool, bool, bool> or = (a, b) => a || b;
        static Func<bool, bool, bool> andFirst = (a, b) => (a && b);
        static Func<bool, bool, bool> orFirst = (a, b) => (a || b);

        /// <summary>
        ///     Initializes a new instance of the <see cref="Rule" /> class.
        /// </summary>
        /// <param name="signals">The technical indicator signals.</param>
        /// <param name="logicalOperators">The logical operators.</param>
        public Rule(ISignal signal)
        {
            _signal = signal;
        }

        /// <summary>
        ///     Gets a value indicating whether this instance is ready.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is ready; otherwise, <c>false</c>.
        /// </value>
        public bool IsReady()
        {
            return IsReady(_signal);
        }

        private bool IsReady(ISignal signal)
        {
            return signal.IsReady && signal.Child == null || IsReady(signal.Child);
        }

        /// <summary>
        ///     Dynamically evaluates the chain of <see cref="ISignal" /> and logical operators.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if the chain of <see cref="ISignal" /> and logical operators is true; otherwise,
        ///     <c>false</c>.
        /// </returns>
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