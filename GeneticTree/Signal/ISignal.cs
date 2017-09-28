using System.Collections.Generic;

namespace GeneticTree.Signal
{
    /// <summary>
    ///     Interface used by the <see cref="Rule" /> class to flag technical indicator signals as crossing moving
    ///     averages or oscillators crossing its thresholds.
    /// </summary>
    public interface ISignal
    {
        /// <summary>
        ///     Gets a value indicating whether this instance is ready.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is ready; otherwise, <c>false</c>.
        /// </value>
        bool IsReady { get; }

        /// <summary>
        ///     Gets the signal. Only used if the instance will be part of a <see cref="Rule" /> class.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if the actual <see cref="Signal" /> correspond with the instance <see cref="TradeRuleDirection" />.
        ///     <c>false</c>
        ///     otherwise.
        /// </returns>
        bool IsTrue();

        ISignal Sibling { get; set; }

        Operator Operator { get; set; }

        ISignal Child { get; set; }

    }

    /// <summary>
    ///     The <see cref="TradingStrategiesBasedOnGeneticAlgorithms" /> implementation requires a direction for every
    ///     technical indicator.
    /// </summary>
    public enum TradeRuleDirection
    {
        LongOnly = 1,
        ShortOnly = -1
    }

    /// <summary>
    ///     List of the technical indicator implemented... well not really, Bollinger bands wasn't implemented.
    /// </summary>
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
        AND,
        OR,
        //todo: not operator
        NOT
    }

    public enum Relationship
    {
        Sibling = 0,
        Child = 1
    }

}