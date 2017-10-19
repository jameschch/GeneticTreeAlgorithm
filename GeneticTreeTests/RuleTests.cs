using NUnit.Framework;
using GeneticTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using GeneticTree.Signal;
using DynamicExpresso;

namespace GeneticTree.Tests
{

    [TestFixture()]
    public class RuleTests
    {

        //[TestCase(new[] { Operator.And }, new[] { true, true }, true, "true && true")]
        //[TestCase(new[] { Operator.Or }, new[] { true, false }, true, "true || false")]
        //[TestCase(new[] { Operator.Or }, new[] { false, false }, false, "false || false")]

        //[TestCase(new[] { Operator.And, Operator.And }, new[] { true, true, true }, true, "true && true && true")]
        //[TestCase(new[] { Operator.And, Operator.And }, new[] { true, true, false }, false, "true && true && false")]

        [TestCase(new[] { Operator.And, Operator.Or  }, new[] { false, true, true }, true, "false && true || true")]
        [TestCase(new[] { Operator.And, Operator.OrInclusive }, new[] { false, true, true }, false, "false && (true || true)")]

        //[TestCase(new[] { Operator.And, Operator.Or }, new[] { true, false, true }, true, "true && (false || true)")]
        //[TestCase(new[] { Operator.And, Operator.Or }, new[] { true, false, false }, false, "true && (false || false)")]

        //[TestCase(new[] { Operator.And, Operator.Or, Operator.And }, new[] { true, true, false, false }, true, "true && true || false && false")]
        //[TestCase(new[] { Operator.And, Operator.Or, Operator.And }, new[] { true, false, false, false }, false, "true && false || false && false")]
        //[TestCase(new[] { Operator.Or, Operator.And, Operator.Or }, new[] { true, false, true, false }, true, "true || false && (true || false)")]
        //[TestCase(new[] { Operator.Or, Operator.And, Operator.Or }, new[] { true, false, false, false }, true, "(true || false && (false || false))")]
        //[TestCase(new[] { Operator.Or, Operator.And, Operator.And }, new[] { true, false, false, false }, true, "(true || false && false && false)")]
        //[TestCase(new[] { Operator.Or, Operator.And, Operator.And }, new[] { true, false, true, true }, true, "(true || false && true && true)")]

        //[TestCase(new[] { Operator.And, Operator.Not }, new[] { true, true, true }, false, "true && true && !true")]
        //[TestCase(new[] { Operator.And, Operator.Nor }, new[] { true, true, true }, true, "true && true || !true")]
        //[TestCase(new[] { Operator.And, Operator.Nor }, new[] { true, true, true }, true, "true && true || !false")]
        //[TestCase(new[] { Operator.And, Operator.NorInclusive }, new[] { true, false, false }, true, "true && (false || !false)")]
        //[TestCase(new[] { Operator.And, Operator.NorInclusive }, new[] { true, false, true }, false, "true && (false || !true)")]

        public void IsTrueTest(Operator[] operators, bool[] values, bool expected, string expression)
        {
            Mock<ISignal> current = new Mock<ISignal>();
            Mock<ISignal> root = current;
            List<ISignal> list = new List<ISignal>();

            for (var i = 0; i < operators.Count(); i++)
            {
                Mock<ISignal> next = new Mock<ISignal>();
                CreateMock(current, next, operators[i], values[i]);
                list.Add(current.Object);
                current = next;
            }

            current.Setup(p => p.IsTrue()).Returns(values.Last());

            var unit = new Rule(list);

            var actual = unit.IsTrue();

            Assert.AreEqual(expected, actual, expression);

            var interpreter = new Interpreter();
            var expressoActual = interpreter.Eval(expression);

            Assert.AreEqual(expressoActual, actual, expression);
        }

        private void CreateMock(Mock<ISignal> current, Mock<ISignal> next, Operator op, bool value)
        {
            current.Setup(p => p.Sibling).Returns(next.Object);
            current.Setup(p => p.Operator).Returns(op);
            current.Setup(p => p.IsTrue()).Returns(value);
        }

        [Test()]
        public void IsReadyTest()
        {

            var current = new Mock<ISignal>();
            current.Setup(c => c.IsReady).Returns(true);
            var all = new List<Mock<ISignal>> { current };

            for (var i = 0; i < 5; i++)
            {
                Mock<ISignal> next = new Mock<ISignal>();
                CreateMock(current, next, Operator.And, true);
                next.Setup(c => c.IsReady).Returns(true);
                all.Add(next);

                current = next;
            }

            var rule = new Rule(all.Select(a => a.Object));

            Assert.IsTrue(rule.IsReady());

            all.Last().Setup(a => a.IsReady).Returns(false);

            Assert.IsFalse(rule.IsReady());
        }

    }
}