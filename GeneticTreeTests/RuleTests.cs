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

        [TestCase(new[] { 1 }, new[] { Operator.AND }, new[] { true, true }, true, "true && true")]
        [TestCase(new[] { 1 }, new[] { Operator.OR }, new[] { true, false }, true, "true || false")]
        [TestCase(new[] { 1 }, new[] { Operator.OR }, new[] { false, false }, false, "false || false")]

        [TestCase(new[] { 1, 1 }, new[] { Operator.AND, Operator.AND }, new[] { true, true, true }, true, "true && true && true")]
        [TestCase(new[] { 1, 1 }, new[] { Operator.AND, Operator.AND }, new[] { true, true, false }, false, "true && true && false")]
        [TestCase(new[] { 1, 1 }, new[] { Operator.AND, Operator.OR }, new[] { true, false, true }, true, "true && false || true")]
        [TestCase(new[] { 0, 1 }, new[] { Operator.AND, Operator.OR }, new[] { true, false, true }, true, "true && (false || true)")]
        [TestCase(new[] { 0, 1 }, new[] { Operator.AND, Operator.OR }, new[] { true, false, false }, false, "true && (false || false)")]

        [TestCase(new[] { 1, 1, 0 }, new[] { Operator.AND, Operator.OR, Operator.AND }, new[] { true, true, false, false }, true, "true && true || (false && false)")]
        [TestCase(new[] { 1, 1, 0 }, new[] { Operator.AND, Operator.OR, Operator.AND }, new[] { true, false, false, false }, false, "true && false || (false && false)")]
        [TestCase(new[] { 1, 1, 0 }, new[] { Operator.OR, Operator.AND, Operator.OR }, new[] { true, false, true, false }, true, "true || false && (true || false)")]
        [TestCase(new[] { 0, 1, 0 }, new[] { Operator.OR, Operator.AND, Operator.OR }, new[] { true, false, false, false }, true, "(true || false && (false || false))")]
        [TestCase(new[] { 0, 1, 0 }, new[] { Operator.OR, Operator.AND, Operator.AND }, new[] { true, false, false, false }, true, "(true || false && (false && false))")]
        [TestCase(new[] { 0, 1, 0 }, new[] { Operator.OR, Operator.AND, Operator.AND }, new[] { true, false, true, true }, true, "(true || false && (true && true))")]
        public void IsTrueTest(int[] relationships, Operator[] operators, bool[] values, bool expected, string expression)
        {
            Mock<ISignal> current = new Mock<ISignal>();
            Mock<ISignal> root = current;

            for (var i = 0; i < relationships.Count(); i++)
            {
                Mock<ISignal> next = new Mock<ISignal>();
                CreateMock(current, next, relationships[i], operators[i], values[i]);
                current = next;
            }

            current.Setup(p => p.IsTrue()).Returns(values.Last());

            var unit = new Rule(root.Object);

            var actual = unit.IsTrue();

            Assert.AreEqual(expected, actual, expression);

            var interpreter = new Interpreter();
            var expressoActual = interpreter.Eval(expression);

            Assert.AreEqual(expressoActual, actual, expression);
        }

        private void CreateMock(Mock<ISignal> current, Mock<ISignal> next, int relationship, Operator @operator, bool value)
        {

            if ((Relationship)relationship == Relationship.Child)
            {
                current.Setup(p => p.Child).Returns(next.Object);
            }
            else
            {
                current.Setup(p => p.Sibling).Returns(next.Object);
            }

            current.Setup(p => p.Operator).Returns(@operator);

            current.Setup(p => p.IsTrue()).Returns(value);
        }

    }
}