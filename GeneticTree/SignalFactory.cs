using System;
using System.Collections.Generic;
using QuantConnect.Configuration;
using QuantConnect.Indicators;
using QuantConnect;
using QuantConnect.Algorithm;
using GeneticTree.Signal;

namespace GeneticTree
{

    public class SignalFactory : AbstractSignalFactory
    {

        private readonly int _maximumSignals = 5;
        int period;
        int slowPeriod;
        int fastPeriod;
        int signalPeriod;
        QCAlgorithm _algorithm;


        public override Rule Create(QCAlgorithm algorithm, Symbol pair, bool isEntryRule)
        {
            _algorithm = algorithm;
            var entryOrExit = isEntryRule ? "Entry" : "Exit";

            period = GetConfigValue("period");
            slowPeriod = GetConfigValue("slowPeriod");
            fastPeriod = GetConfigValue("fastPeriod");
            signalPeriod = GetConfigValue("signalPeriod");

            ISignal root = null;
            ISignal parent = null;

            for (var i = 1; i <= _maximumSignals; i++)
            {
                var item = CreateIndicator(pair, i, entryOrExit);
                if (root == null)
                {
                    root = item;
                    parent = root;
                }
                else
                {
                    //root won't have parent to add to
                    if ((Relationship)GetConfigValue(entryOrExit + "Relationship" + (i - 1)) == Relationship.Child)
                    {
                        parent.Child = item;
                    }
                    else
                    {
                        parent.Sibling = item;
                    }
                }

                //last item won't have operator
                if (i < _maximumSignals)
                {
                    var key = entryOrExit + "Operator" + i;
                    Operator op = (Operator)GetConfigValue(key);
                    item.Operator = op;
                }

                parent = item;
            }

            return new Rule(root);
        }

        protected override ISignal CreateIndicator(Symbol pair, int i, string entryOrExit)
        {
            var oscillatorThresholds = new OscillatorThresholds { Lower = 20, Upper = 80 };
            var key = entryOrExit + "Indicator" + i + "Direction";
            var intDirection = GetConfigValue(key);

            var direction = intDirection == 0 ? TradeRuleDirection.LongOnly : TradeRuleDirection.ShortOnly;

            key = entryOrExit + "Indicator" + i;

            var indicator = (TechicalIndicators)GetConfigValue(key);
            ISignal signal = null;

            switch (indicator)
            {
                case TechicalIndicators.SimpleMovingAverage:
                    var fast = _algorithm.SMA(pair, period, Resolution.Hour);
                    var slow = _algorithm.SMA(pair, period, Resolution.Hour);
                    signal = new CrossingMovingAverageSignal(fast, slow, direction);
                    break;

                case TechicalIndicators.MovingAverageConvergenceDivergence:
                    var macd = _algorithm.MACD(pair, fastPeriod, slowPeriod, signalPeriod, MovingAverageType.Simple, Resolution.Hour);
                    signal = new CrossingMovingAverageSignal(macd, macd.Signal, direction);
                    break;

                case TechicalIndicators.Stochastic:
                    var sto = _algorithm.STO(pair, period, Resolution.Hour);
                    signal = new OscillatorSignal(sto.StochD, oscillatorThresholds, direction);
                    break;

                case TechicalIndicators.RelativeStrengthIndex:
                    var rsi = _algorithm.RSI(pair, period);
                    signal = new OscillatorSignal(rsi, oscillatorThresholds, direction);
                    break;

                case TechicalIndicators.CommodityChannelIndex:
                    var cci = _algorithm.CCI(pair, period, MovingAverageType.Simple, Resolution.Hour);
                    oscillatorThresholds.Lower = -100;
                    oscillatorThresholds.Lower = 100;
                    signal = new OscillatorSignal(cci, oscillatorThresholds, direction);
                    break;

                case TechicalIndicators.MomentumPercent:
                    var pm = _algorithm.MOMP(pair, period, Resolution.Hour);
                    oscillatorThresholds.Lower = -5;
                    oscillatorThresholds.Lower = 5;
                    signal = new OscillatorSignal(pm, oscillatorThresholds, direction);
                    break;

                case TechicalIndicators.WilliamsPercentR:
                    var wr = _algorithm.WILR(pair, period, Resolution.Hour);
                    signal = new OscillatorSignal(wr, oscillatorThresholds, direction);
                    break;

                case TechicalIndicators.PercentagePriceOscillator:
                    var ppo = _algorithm.MACD(pair, fastPeriod, slowPeriod, signalPeriod, MovingAverageType.Simple, Resolution.Hour).Over(_algorithm.EMA(pair, period, resolution: Resolution.Hour))
                        .Plus(constant: 100m);
                    var compound = new SimpleMovingAverage(period).Of(ppo);
                    signal = new CrossingMovingAverageSignal(ppo, compound, direction);
                    break;

                case TechicalIndicators.None:
                    signal = new EmptySignal();
                    break;

                case TechicalIndicators.BollingerBands:
                    throw new NotImplementedException("WIP");
            }

            return signal;
        }

        protected override int GetConfigValue(string key)
        {
            int value;
            try
            {
                value = Config.GetInt(key, int.Parse(_algorithm.GetParameter(key)));
                _algorithm.Log(string.Format("Parameter {0} set to {1}", key, value));
            }
            catch (ArgumentNullException e)
            {
                throw new ArgumentNullException(key,
                    "The gene " + key + " is not present either as Config or as Parameter");
            }

            return value;
        }
    }

    public abstract class AbstractSignalFactory
    {
        public abstract Rule Create(QCAlgorithm algorithm, Symbol pair, bool isEntryRule);
        protected abstract ISignal CreateIndicator(Symbol pair, int i, string entryOrExit);
        protected abstract int GetConfigValue(string key);
    }

}