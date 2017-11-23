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
using QuantConnect.Data;
using System.Dynamic;
using NodaTime;
using QuantConnect.Indicators;

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

            fakeAlgorithm.SubscriptionManager = new SubscriptionManagerWrapper(fakeSymbol, null, null);

            //todo: remaining indicators
            var expectedEntry = new[] { "None", "SimpleMovingAverage", "MovingAverageConvergenceDivergence", "Stochastic", "RelativeStrengthIndex" };

            var expectedExit = new[] {  "CommodityChannelIndex", "MomentumPercent", "WilliamsPercentR", "PercentagePriceOscillator", "AverageDirectionalIndex" };

            var actual = unit.Create(fakeAlgorithm, fakeSymbol, true);

            Assert.AreEqual(Operator.And, actual.List.First().Operator);
            Assert.AreEqual(Operator.Or, actual.List.First().Child.Operator);
            Assert.AreEqual(Operator.OrInclusive, actual.List.First().Child.Child.Operator);
            Assert.AreEqual(Operator.Not, actual.List.First().Child.Child.Child.Operator);

            CollectionAssert.AreEquivalent(expectedEntry, actual.List.Select(l => l.Name));

            actual = unit.Create(fakeAlgorithm, fakeSymbol, false);

            Assert.AreEqual(Operator.Nor, actual.List.First().Operator);
            Assert.AreEqual(Operator.NorInclusive, actual.List.First().Child.Operator);
            Assert.AreEqual(Operator.And, actual.List.First().Child.Child.Operator);
            Assert.AreEqual(Operator.Or, actual.List.First().Child.Child.Child.Operator);

            CollectionAssert.AreEquivalent(expectedExit, actual.List.Select(l => l.Name));

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
                {"ExitIndicator1",  4},
                {"ExitIndicator2",  5},
                {"ExitIndicator3",  6},
                {"ExitIndicator4",  7},
                {"ExitIndicator5",  8},
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

            protected override int GetConfigValue(string key)
            {
                return config[key];
            }

        }

        protected class SubscriptionManagerWrapper : SubscriptionManager
        {
            public SubscriptionManagerWrapper(Symbol symbol, AlgorithmSettings algorithmSettings, TimeKeeper timeKeeper) : base(algorithmSettings, timeKeeper)
            {

                this.Subscriptions = new HashSet<SubscriptionDataConfig> { new SubscriptionDataConfig(typeof(BaseData), symbol, Resolution.Hour, DateTimeZone.Utc,
                    DateTimeZone.Utc, false, false, false, false, TickType.Quote) };
            }



        }

    }
}