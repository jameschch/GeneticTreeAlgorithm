using DynamicExpresso;
using GeneticTree.Signal;
using QuantConnect.Data;
using QuantConnect.Indicators;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeneticTree
{

    public class Rule
    {

        public IEnumerable<ISignal> List { get; }

        public Rule(IEnumerable<ISignal> signal)
        {
            List = signal;
        }

        public bool IsReady()
        {
            return List.All(s => s.IsReady);
        }

        public bool IsTrue()
        {
            var interpreter = new Interpreter();

            StringBuilder expression = new StringBuilder();

            foreach (var item in List)
            {

                string isTrue = item.IsTrue().ToString().ToLower();

                if (new[] { Operator.NorInclusive, Operator.OrInclusive }.Contains(item.Operator))
                {
                    isTrue = "(" + isTrue;
                }

                if (item.Parent != null && new[] { Operator.NorInclusive, Operator.OrInclusive }.Contains(item.Parent.Operator))
                {
                    isTrue += ")";
                }

                expression.Append(isTrue);

                if (item.Child != null)
                {
                    if (item.Operator == Operator.And)
                    {
                        expression.Append(" && ");
                    }
                    else if (new[] { Operator.Or, Operator.OrInclusive }.Contains(item.Operator))
                    {
                        expression.Append(" || ");
                    }
                    else if (item.Operator == Operator.Not)
                    {
                        expression.Append(" && !");
                    }
                    else if (new[] { Operator.Nor, Operator.NorInclusive }.Contains(item.Operator))
                    {
                        expression.Append(" || !");
                    }
                }

            }

            return (bool)interpreter.Eval(expression.ToString());
        }

        public void Update(BaseData data)
        {
            List.First().Update(data);
        }

    }
}