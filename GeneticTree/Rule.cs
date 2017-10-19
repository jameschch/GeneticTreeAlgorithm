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

            string expression = "";

            foreach (var item in List)
            {

                bool isTrue = item.IsTrue();
                bool nextTrue = item.Sibling != null ? item.Sibling.IsTrue() : true;

                if (List.First() == item)
                {

                }

                if (item.Operator == Operator.And)
                {
                    expression += $" {isTrue.ToString().ToLower()} && {nextTrue.ToString().ToLower()} ";
                }
                else if (item.Operator == Operator.Or)
                {
                    expression += $" {isTrue.ToString().ToLower()} || {nextTrue.ToString().ToLower()} ";
                }
                else if (item.Operator == Operator.Not)
                {
                    expression += $" {isTrue.ToString().ToLower()} && !{nextTrue.ToString().ToLower()} ";
                }
                else if (item.Operator == Operator.Nor)
                {
                    expression += $" ({isTrue.ToString().ToLower()} || !{nextTrue.ToString().ToLower()}) ";
                }
                else if (item.Operator == Operator.OrInclusive)
                {
                    expression += $" ({isTrue.ToString().ToLower()} || {nextTrue.ToString().ToLower()}) ";
                }
                else if (item.Operator == Operator.NorInclusive)
                {
                    expression += $" ({isTrue.ToString().ToLower()} || !{nextTrue.ToString().ToLower()}) ";
                }

            }

            return (bool)interpreter.Eval(expression);          
        }

        public void Update(BaseData data)
        {
            List.First().Update(data);
        }

    }
}