using System;
using QuantConnect.Indicators;
namespace GeneticTree.Signal
{
    /// <summary>
    ///     This class keeps track of an oscillator respect to its thresholds and updates an <see cref="BBOscillatorSignal" />
    ///     for each given state.
    /// </summary>
    /// <seealso cref="QuantConnect.Algorithm.CSharp.ITechnicalIndicatorSignal" />
    public class BBOscillatorSignal : OscillatorSignal
    {
        private BollingerBands _bb;

        /// <summary>
        ///     Initializes a new instance of the <see cref="BBOscillatorSignal" /> class.
        /// </summary>
        /// <param name="indicator">The indicator.</param>
        /// <remarks>The oscillator must be registered BEFORE being used by this constructor.</remarks>
        public BBOscillatorSignal(BollingerBands indicator, Direction direction, int survivalPeriod = 1) 
		: base(indicator, direction, survivalPeriod: survivalPeriod)
        {
            _bb = indicator;
        }

        /// <summary>
        ///     Gets the actual position respect to the thresholds.
        /// </summary>
        /// <param name="indicatorCurrentValue">The indicator current value.</param>
        /// <returns></returns>
        protected override ThresholdState GetThresholdState(decimal indicatorCurrentValue)
        {
            var positionSignal = ThresholdState.InBetween;
            if (indicatorCurrentValue > _bb.UpperBand)
            {
                positionSignal = ThresholdState.AboveUpper;
            }
            else if (indicatorCurrentValue < _bb.LowerBand)
            {
                positionSignal = ThresholdState.BelowLower;
            }
            return positionSignal;
        }
    }
}