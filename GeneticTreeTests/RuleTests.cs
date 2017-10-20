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

        [TestCase(new[] { Operator.And }, new[] { true, true }, true, "true && true")]
        [TestCase(new[] { Operator.Or }, new[] { true, false }, true, "true || false")]
        [TestCase(new[] { Operator.Or }, new[] { false, false }, false, "false || false")]

        [TestCase(new[] { Operator.And, Operator.And }, new[] { true, true, true }, true, "true && true && true")]
        [TestCase(new[] { Operator.And, Operator.And }, new[] { true, true, false }, false, "true && true && false")]

        [TestCase(new[] { Operator.And, Operator.Or }, new[] { false, true, true }, true, "false && true || true")]
        [TestCase(new[] { Operator.And, Operator.OrInclusive }, new[] { false, true, true }, false, "false && (true || true)")]

        [TestCase(new[] { Operator.And, Operator.Or, Operator.And, Operator.Or }, new[] { false, false, true, false, true }, true, "false && false || true && false || true")]
        [TestCase(new[] { Operator.And, Operator.OrInclusive, Operator.And, Operator.OrInclusive }, new[] { false, false, true, false, true }, false, "false && (false || true) && (false || true)")]

        [TestCase(new[] { Operator.And, Operator.Not }, new[] { true, true, true }, false, "true && true && !true")]

        [TestCase(new[] { Operator.And, Operator.Nor }, new[] { true, true, true }, true, "true && true || !true")]
        [TestCase(new[] { Operator.And, Operator.Nor }, new[] { true, true, false }, true, "true && true || !false")]

        [TestCase(new[] { Operator.And, Operator.Nor }, new[] { false, true, false }, true, "false && true || !false")]
        [TestCase(new[] { Operator.And, Operator.NorInclusive }, new[] { false, true, false }, false, "false && (true || !false)")]

        public void IsTrueTest(Operator[] operators, bool[] values, bool expected, string expression)
        {

            Mock<ISignal> parent = null;
            List<ISignal> list = new List<ISignal>();

            for (var i = 0; i < values.Length; i++)
            {
                Mock<ISignal> current = new Mock<ISignal>();
                Mock<ISignal> child = i == values.Length-1 ? null : new Mock<ISignal>();
                CreateMock(current, child, parent, i < operators.Length ? operators[i] : new Nullable<Operator>(), values[i]);
                parent = current;
                list.Add(current.Object);
            }

            var unit = new Rule(list);

            var actual = unit.IsTrue();

            Assert.AreEqual(expected, actual, expression);

            var interpreter = new Interpreter();
            var expressoActual = interpreter.Eval(expression);

            Assert.AreEqual(expressoActual, actual, expression);
        }

        private void CreateMock(Mock<ISignal> current, Mock<ISignal> child, Mock<ISignal> parent, Operator? op, bool value)
        {
            if (child != null) current.Setup(p => p.Child).Returns(child.Object);
            if (parent != null) current.Setup(p => p.Parent).Returns(parent.Object);
            if (op.HasValue) current.Setup(p => p.Operator).Returns(op.Value);
            current.Setup(p => p.IsTrue()).Returns(value);
        }

        [Test()]
        public void IsReadyTest()
        {

            var all = new List<Mock<ISignal>>();
            var current = new Mock<ISignal>();

            for (var i = 0; i < 5; i++)
            {
                current.Setup(c => c.IsReady).Returns(true);
                Mock<ISignal> next = new Mock<ISignal>();
                CreateMock(current, next, null, Operator.And, true);
                all.Add(current);
                current = next;
            }

            var rule = new Rule(all.Select(a => a.Object));

            Assert.IsTrue(rule.IsReady());

            all.Last().Setup(a => a.IsReady).Returns(false);

            Assert.IsFalse(rule.IsReady());
        }

    }
}