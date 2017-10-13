using QuantConnect.Data;
using QuantConnect.Indicators;
using System.Collections.Generic;

namespace GeneticTree.Signal
{


    public interface ISignal
    {
        /// <summary>
        ///     Gets a value indicating whether this instance is ready.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is ready; otherwise, <c>false</c>.
        /// </value>
        bool IsReady { get; }

        bool IsTrue();

        ISignal Sibling { get; set; }

        Operator Operator { get; set; }

        void Update(BaseData data);

    }

    public enum TradeRuleDirection
    {
        LongOnly = 1,
        ShortOnly = -1
    }

    public enum TechicalIndicators
    {
        None = -1,
        SimpleMovingAverage = 0,
        MovingAverageConvergenceDivergence = 1,
        Stochastic = 2,
        RelativeStrengthIndex = 3,
        CommodityChannelIndex = 4,
        MomentumPercent = 5,
        WilliamsPercentR = 6,
        PercentagePriceOscillator = 7,
        BollingerBands = 8
    }

    public enum Operator
    {
        And = 0,
        Or = 1,
        OrInclusive = 2,
        Not = 3,
        Nor = 4,
        NorInclusive = 5
    }

}