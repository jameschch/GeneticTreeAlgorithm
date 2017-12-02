using System;
using QuantConnect.Indicators;

namespace GeneticTree.Signal
    {
        /// <summary>
    ///     This class keeps track of an oscillator respect to its thresholds and updates an <see cref="ChannelOscillatorSignal" />
        ///     for each given state.
        /// </summary>
        /// <seealso cref="QuantConnect.Algorithm.CSharp.ITechnicalIndicatorSignal" />
        public class ChannelOscillatorSignal : OscillatorSignal
        {

        /// <summary>
        ///     Gets a value indicating whether this instance is ready.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is ready; otherwise, <c>false</c>.
        /// </value>
        public override bool IsReady
        {
            get { return Indicator.IsReady; }
        }
            /// <summary>
        ///     Initializes a new instance of the <see cref="ChannelOscillatorSignal" /> class.
            /// </summary>
            /// <param name="indicator">The indicator.</param>
            /// <remarks>The oscillator must be registered BEFORE being used by this constructor.</remarks>
            public ChannelOscillatorSignal(IndicatorBase<IndicatorDataPoint> price, IndicatorBase<IndicatorDataPoint> max, IndicatorBase<IndicatorDataPoint> min, Direction direction, int survivalPeriod) 
			: base(price, direction, survivalPeriod)
            {

                max.Updated += new IndicatorUpdatedHandler(Max_Updated);
                min.Updated += new IndicatorUpdatedHandler(Min_Updated);

            }
            private void Max_Updated(object sender, IndicatorDataPoint updated)
            {
                var positionSignal = ThresholdState.InBetween;
                if (IsReady)
                {
                    if (((IndicatorBase<IndicatorDataPoint>)Indicator).Current.Value > updated.Value)
                    {
                        positionSignal = ThresholdState.AboveUpper;
                    }
                }
                ProcessThresholdStateChange(positionSignal,updated);
            }

            private void Min_Updated(object sender, IndicatorDataPoint updated)
            {
                var positionSignal = ThresholdState.InBetween;
                if (IsReady)
                {
                    if (((IndicatorBase<IndicatorDataPoint>)Indicator).Current.Value < updated.Value)
                    {
                        positionSignal = ThresholdState.BelowLower;
                    }
                }
                ProcessThresholdStateChange(positionSignal, updated);
            }

            protected override void Indicator_Updated(object sender, IndicatorDataPoint updated)
            {
                //ignore  , we already have an indicator  
            }

            /// <summary>
            ///     Gets the signal. Only used if the instance will be part of a <see cref="TradingRule" /> class.
            /// </summary>
            /// <returns>
            ///     <c>true</c> if the actual <see cref="Signal" /> correspond with the instance <see cref="TradeRuleDirection" />.
            ///     <c>false</c>
            ///     otherwise.
            /// </returns>
            public override bool IsTrue()
            {
                var signal = false;
                if (IsReady)
                {
                    switch (_direction)
                    {
                    case Direction.LongOnly:
                        signal = Signal == ThresholdState.AboveUpper;
                            break;

                    case Direction.ShortOnly:
                        signal = Signal == ThresholdState.BelowLower;
                            break;

                    }
                }
                return signal;
            }
        }
    }