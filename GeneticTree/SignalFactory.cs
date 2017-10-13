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
        int _period;
        int _slowPeriod;
        int _fastPeriod;
        int _signalPeriod;
        QCAlgorithm _algorithm;
        Resolution _resolution;
        private bool _enableParameterLog = false;

        public override Rule Create(QCAlgorithm algorithm, Symbol pair, bool isEntryRule, Resolution resolution = Resolution.Hour)
        {
            _algorithm = algorithm;
            _resolution = resolution;
            var entryOrExit = isEntryRule ? "Entry" : "Exit";

            _period = GetConfigValue("period");
            _slowPeriod = GetConfigValue("slowPeriod");
            _fastPeriod = GetConfigValue("fastPeriod");
            _signalPeriod = GetConfigValue("signalPeriod");

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
                    parent.Sibling = item;
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
                    var fast = _algorithm.SMA(pair, _period, _resolution);
                    var slow = _algorithm.SMA(pair, _period, _resolution);
                    signal = new CrossingMovingAverageSignal(fast, slow, direction);
                    break;

                case TechicalIndicators.MovingAverageConvergenceDivergence:
                    var macd = _algorithm.MACD(pair, _fastPeriod, _slowPeriod, _signalPeriod, MovingAverageType.Simple, _resolution);
                    signal = new CrossingMovingAverageSignal(macd, macd.Signal, direction);
                    break;

                case TechicalIndicators.Stochastic:
                    var sto = _algorithm.STO(pair, _period, _resolution);
                    signal = new OscillatorSignal(sto.StochD, oscillatorThresholds, direction);
                    break;

                case TechicalIndicators.RelativeStrengthIndex:
                    var rsi = _algorithm.RSI(pair, _period);
                    signal = new OscillatorSignal(rsi, oscillatorThresholds, direction);
                    break;

                case TechicalIndicators.CommodityChannelIndex:
                    var cci = _algorithm.CCI(pair, _period, MovingAverageType.Simple, _resolution);
                    oscillatorThresholds.Lower = -100;
                    oscillatorThresholds.Lower = 100;
                    signal = new OscillatorSignal(cci, oscillatorThresholds, direction);
                    break;

                case TechicalIndicators.MomentumPercent:
                    var pm = _algorithm.MOMP(pair, _period, _resolution);
                    oscillatorThresholds.Lower = -5;
                    oscillatorThresholds.Lower = 5;
                    signal = new OscillatorSignal(pm, oscillatorThresholds, direction);
                    break;

                case TechicalIndicators.WilliamsPercentR:
                    var wr = _algorithm.WILR(pair, _period, _resolution);
                    signal = new OscillatorSignal(wr, oscillatorThresholds, direction);
                    break;

                case TechicalIndicators.PercentagePriceOscillator:
                    var ppo = _algorithm.MACD(pair, _fastPeriod, _slowPeriod, _signalPeriod, MovingAverageType.Simple, _resolution).Over(_algorithm.EMA(pair, _period, resolution: _resolution))
                        .Plus(constant: 100m);
                    var compound = new SimpleMovingAverage(_period).Of(ppo);
                    signal = new CrossingMovingAverageSignal(ppo, compound, direction);
                    break;

                case TechicalIndicators.None:
                    signal = new EmptySignal();
                    break;

                case TechicalIndicators.BollingerBands:
                    //todo: bollinger bands setup
                    throw new NotImplementedException("WIP");
            }

            return signal;
        }

        protected override int GetConfigValue(string key)
        {
            int value;
            try
            {
                int.TryParse(_algorithm.GetParameter(key), out value);
                value = Config.GetInt(key, value);
                if (_enableParameterLog)
                {
                    _algorithm.Log(string.Format("Parameter {0} set to {1}", key, value));
                }
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
        public abstract Rule Create(QCAlgorithm algorithm, Symbol pair, bool isEntryRule, Resolution resolution = Resolution.Hour);
        protected abstract ISignal CreateIndicator(Symbol pair, int i, string entryOrExit);
        protected abstract int GetConfigValue(string key);
    }

}