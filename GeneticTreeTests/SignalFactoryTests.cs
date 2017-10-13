using NUnit.Framework;
using GeneticTree;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using GeneticTree.Signal;
using QuantConnect;
using QuantConnect.Algorithm;
using System.Runtime.Serialization;

namespace GeneticTree.Tests
{
    [TestFixture()]
    public class SignalFactoryTests
    {
        [Test()]
        public void CreateTest()
        {
            var unit = new SignalFactoryWrapper();

            var fakeAlgorithm = (QCAlgorithm)FormatterServices.GetUninitializedObject(typeof(QCAlgorithm));

            var fakeSymbol = Symbol.Create("ABCXYZ", SecurityType.Equity, Market.USA);

            var actual = unit.Create(fakeAlgorithm, fakeSymbol, true);

            Assert.AreEqual(Operator.And, actual.Signal.Operator);
            Assert.AreEqual(Operator.Or, actual.Signal.Sibling.Operator);
            Assert.AreEqual(Operator.OrInclusive, actual.Signal.Sibling.Sibling.Operator);
            Assert.AreEqual(Operator.Not, actual.Signal.Sibling.Sibling.Sibling.Operator);

            actual = unit.Create(fakeAlgorithm, fakeSymbol, false);

            Assert.AreEqual(Operator.Nor, actual.Signal.Operator);
            Assert.AreEqual(Operator.NorInclusive, actual.Signal.Sibling.Operator);
            Assert.AreEqual(Operator.And, actual.Signal.Sibling.Sibling.Operator);
            Assert.AreEqual(Operator.Or, actual.Signal.Sibling.Sibling.Sibling.Operator);
        }

        protected class SignalFactoryWrapper : SignalFactory
        {

            private static Dictionary<string, int> config = new Dictionary<string, int> {
                {"EntryIndicator1",  0},
                {"EntryIndicator2",  1},
                {"EntryIndicator3",  -1},
                {"EntryIndicator4",  2},
                {"EntryIndicator5",  3},
                {"EntryIndicator1Direction",  0},
                {"EntryIndicator2Direction",  0},
                {"EntryIndicator3Direction",  1},
                {"EntryIndicator4Direction",  0},
                {"EntryIndicator5Direction",  1},
                {"EntryOperator1",  0},
                {"EntryOperator2",  1},
                {"EntryOperator3",  2},
                {"EntryOperator4",  3},
                {"ExitIndicator1",  6},
                {"ExitIndicator2",  5},
                {"ExitIndicator3",  4},
                {"ExitIndicator4",  -1},
                {"ExitIndicator5",  2},
                {"ExitIndicator1Direction",  0},
                {"ExitIndicator2Direction",  0},
                {"ExitIndicator3Direction",  1},
                {"ExitIndicator4Direction",  1},
                {"ExitIndicator5Direction",  0},
                {"ExitOperator1",  4},
                {"ExitOperator2",  5},
                {"ExitOperator3",  0},
                {"ExitOperator4",  1},
                {"period",  1},
                {"slowPeriod",  2},
                {"fastPeriod",  3},
                {"signalPeriod",  4 }
            };

            protected override ISignal CreateIndicator(Symbol pair, int i, string entryOrExit)
            {
                return new EmptySignal();
            }

            protected override int GetConfigValue(string key)
            {
                return config[key];
            }

        }

    }
}