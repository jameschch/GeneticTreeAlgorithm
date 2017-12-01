using GeneticTree.BooleanLogicParser;
using GeneticTree.Signal;
using QuantConnect.Data;
using System.Collections.Generic;
using System.Linq;

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
            var expression = "";

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

                expression += isTrue;

                if (item.Child != null)
                {
                    if (item.Operator == Operator.And)
                    {
                        expression += " and ";
                    }
                    else if (new[] { Operator.Or, Operator.OrInclusive }.Contains(item.Operator))
                    {
                        expression += " or ";
                    }
                    else if (item.Operator == Operator.Not)
                    {
                        expression += " and !";
                    }
                    else if (new[] { Operator.Nor, Operator.NorInclusive }.Contains(item.Operator))
                    {
                        expression += " or !";
                    }
                }
            }

            var tokens = new Tokenizer(expression).Tokenize();
            var parser = new Parser(tokens);
            return parser.Parse();
        }

        public void Update(BaseData data)
        {
            List.First().Update(data);
        }

        public string Status()
        {
            var builder = "";
            foreach (var item in List)
            {
                builder += $"Name:{item.Name} Operator:{item.Operator} IsReady:{item.IsReady} IsTrue:{item.IsTrue()}, ";
            }
            return builder.Trim(", ".ToArray());
        }

    }
}