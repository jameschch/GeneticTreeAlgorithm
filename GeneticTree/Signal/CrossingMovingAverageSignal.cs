using System;
using QuantConnect.Indicators;
using QuantConnect.Data;

namespace GeneticTree.Signal
{
    /// <summary>
    ///     Possibles states of two moving averages.
    /// </summary>
    public enum CrossingMovingAveragesSignals
    {
        Bullish = 1,
        FastCrossSlowFromAbove = -2,
        Bearish = -1,
        FastCrossSlowFromBelow = 2
    }

    /// <summary>
    ///     This class keeps track of two crossing moving averages and updates a <see cref="CrossingMovingAveragesSignals" />
    ///     for each given state.
    /// </summary>
    public class CrossingMovingAverageSignal : SignalBase
    {
        private readonly CompositeIndicator<IndicatorDataPoint> _moving_average_difference;
        private readonly Direction _direction;
        private int _lastSignal;
        IndicatorBase<IndicatorDataPoint> _fast { get; set; }
        IndicatorBase<IndicatorDataPoint> _slow { get; set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="CrossingMovingAverageSignal" /> class.
        /// </summary>
        /// <param name="fast">The fast moving average.</param>
        /// <param name="slow">The slow moving average.</param>
        /// <param name="direction">
        ///     The trade rule direction. Only used if the instance will be part of a
        ///     <see cref="Rule" /> class
        /// </param>
        /// <remarks>
        ///     Both Moving Averages must be registered BEFORE being used by this constructor.
        /// </remarks>
        public CrossingMovingAverageSignal(IndicatorBase<IndicatorDataPoint> fast,
            IndicatorBase<IndicatorDataPoint> slow, Direction direction)
        {
            _fast = fast;
            _slow = slow;
            _moving_average_difference = fast.Minus(slow);
            _moving_average_difference.Updated += ma_Updated;
            _direction = direction;
        }

        /// <summary>
        ///     Gets the actual state of both moving averages.
        /// </summary>
        public CrossingMovingAveragesSignals Signal { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is ready.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is ready; otherwise, <c>false</c>.
        /// </value>
        public override bool IsReady
        {
            get { return _moving_average_difference.IsReady; }
        }

        /// <summary>
        ///     Gets the signal. Only used if the instance will be part of a <see cref="Rule" /> class.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if the actual <see cref="Signal" /> correspond with the instance <see cref="Direction" />.
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
                        signal = Signal == CrossingMovingAveragesSignals.FastCrossSlowFromBelow;
                        break;

                    case Direction.ShortOnly:
                        signal = Signal == CrossingMovingAveragesSignals.FastCrossSlowFromAbove;
                        break;
                }
            }
            return signal;
        }

        private void ma_Updated(object sender, IndicatorDataPoint updated)
        {
            if (!IsReady)
            {
                return;
            }
            var actualSignal = Math.Sign(_moving_average_difference);
            if (actualSignal == _lastSignal || _lastSignal == 0)
            {
                Signal = (CrossingMovingAveragesSignals)actualSignal;
            }
            else if (_lastSignal == -1 && actualSignal == 1)
            {
                Signal = CrossingMovingAveragesSignals.FastCrossSlowFromBelow;
            }
            else if (_lastSignal == 1 && actualSignal == -1)
            {
                Signal = CrossingMovingAveragesSignals.FastCrossSlowFromAbove;
            }

            _lastSignal = actualSignal;
        }
        public override void Update(BaseData data)
        {
            var point = new IndicatorDataPoint(data.Time, data.Price);
            AttemptCompositeUpdate(_fast, point);
            AttemptCompositeUpdate(_slow, point);
            _fast.Update(point);
            _slow.Update(point);
            base.Update(data);
        }

        private void AttemptCompositeUpdate(IndicatorBase<IndicatorDataPoint> indicator, IndicatorDataPoint point)
        {
            if (indicator.GetType() == typeof(CompositeIndicator<IndicatorDataPoint>))
            {
                var composite = ((CompositeIndicator<IndicatorDataPoint>)indicator);
                composite.Left.Update(point);
                composite.Right.Update(point);
                AttemptCompositeUpdate(composite.Left, point);
                AttemptCompositeUpdate(composite.Right, point);
            }
        }

    }
}