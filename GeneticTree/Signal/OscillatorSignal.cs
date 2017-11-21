using System;
using QuantConnect.Indicators;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using System.Linq;

namespace GeneticTree.Signal
{

    /// <summary>
    ///     This class keeps track of an oscillator respect to its thresholds and updates an <see cref="OscillatorSignal" />
    ///     for each given state.
    /// </summary>
    /// <seealso cref="QuantConnect.Algorithm.CSharp.ITechnicalIndicatorSignal" />
    public class OscillatorSignal : SignalBase
    {
        private decimal _previousIndicatorValue;
        private ThresholdState _previousSignal;
        private int[] _thresholds;
        private Direction _direction;
        static int[] defaultThresholds = new int[2] { 20, 80 };

        /// <summary>
        ///     Possibles states of an oscillator respect to its thresholds.
        /// </summary>
        public enum ThresholdState
        {
            CrossLowerFromAbove = -3,
            BelowLower = -2,
            CrossLowerFromBelow = -1,
            InBetween = 0,
            CrossUpperFromBelow = 3,
            AboveUpper = 2,
            CrossUpperFromAbove = 1
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OscillatorSignal" /> class.
        /// </summary>
        /// <param name="indicator">The indicator.</param>
        /// <param name="thresholds">The thresholds.</param>
        /// <param name="direction">
        ///     The trade rule direction. Only used if the instance will be part of a
        ///     <see cref="Rule" /> class
        /// </param>
        /// <remarks>The oscillator must be registered BEFORE being used by this constructor.</remarks>
        public OscillatorSignal(dynamic indicator, int[] thresholds, Direction direction)
        {
            Initialize(indicator, thresholds, direction);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OscillatorSignal" /> class.
        /// </summary>
        /// <param name="indicator">The indicator.</param>
        /// <param name="thresholds">The thresholds.</param>
        /// <remarks>The oscillator must be registered BEFORE being used by this constructor.</remarks>
        public OscillatorSignal(dynamic indicator, int[] thresholds)
        {
            Initialize(indicator, thresholds, Direction.LongOnly);
        }

        public OscillatorSignal(dynamic indicator, Direction direction)
        {
            Initialize(indicator, defaultThresholds, direction);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="OscillatorSignal" /> class.
        /// </summary>
        /// <param name="indicator">The indicator.</param>
        /// <remarks>The oscillator must be registered BEFORE being used by this constructor.</remarks>
        public OscillatorSignal(dynamic indicator)
        {
            Initialize(indicator, defaultThresholds, Direction.LongOnly);
        }

        /// <summary>
        ///     The underlying indicator, must be an oscillator.
        /// </summary>
        public dynamic Indicator { get; private set; }

        /// <summary>
        ///     Gets the actual state of the oscillator.
        /// </summary>
        public ThresholdState Signal { get; private set; }

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

        public override string Name { get { return ((string)Indicator.GetType().ToString()).Split('.').Last(); } }

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
                        signal = Signal == ThresholdState.CrossLowerFromBelow;
                        break;

                    case Direction.ShortOnly:
                        signal = Signal == ThresholdState.CrossUpperFromAbove;
                        break;
                }
            }
            return signal;
        }

        /// <summary>
        ///     Updates the <see cref="Signal" /> status.
        /// </summary>
        private void Indicator_Updated(object sender, IndicatorDataPoint updated)
        {
            var actualPositionSignal = GetThresholdState(updated);
            if (!Indicator.IsReady)
            {
                _previousIndicatorValue = updated.Value;
                _previousSignal = actualPositionSignal;
                Signal = _previousSignal;
                return;
            }

            var actualSignal = GetActualSignal(_previousSignal, actualPositionSignal);

            Signal = actualSignal;
            _previousIndicatorValue = updated.Value;
            _previousSignal = actualSignal;
        }

        /// <summary>
        ///     Gets the actual position respect to the thresholds.
        /// </summary>
        /// <param name="indicatorCurrentValue">The indicator current value.</param>
        /// <returns></returns>
        private ThresholdState GetThresholdState(decimal indicatorCurrentValue)
        {
            var positionSignal = ThresholdState.InBetween;
            if (indicatorCurrentValue > _thresholds[1])
            {
                positionSignal = ThresholdState.AboveUpper;
            }
            else if (indicatorCurrentValue < _thresholds[0])
            {
                positionSignal = ThresholdState.BelowLower;
            }
            return positionSignal;
        }

        /// <summary>
        ///     Gets the actual signal from the actual position respect to the thresholds and the last signal.
        /// </summary>
        /// <param name="previousSignal">The previous signal.</param>
        /// <param name="actualPositionSignal">The actual position signal.</param>
        /// <returns></returns>
        private ThresholdState GetActualSignal(ThresholdState previousSignal, ThresholdState actualPositionSignal)
        {
            ThresholdState actualSignal;
            var previous = (int)previousSignal;
            var current = (int)actualPositionSignal;

            if (current == 0)
            {
                if (Math.Abs(previous) > 1)
                {
                    actualSignal = (ThresholdState)Math.Sign(previous);
                }
                else
                {
                    actualSignal = ThresholdState.InBetween;
                }
            }
            else
            {
                if (previous * current <= 0 || Math.Abs(previous + current) == 3)
                {
                    actualSignal = (ThresholdState)(Math.Sign(current) * 3);
                }
                else
                {
                    actualSignal = (ThresholdState)(Math.Sign(current) * 2);
                }
            }
            return actualSignal;
        }

        /// <summary>
        /// Sets up class.
        /// </summary>
        /// <param name="indicator">The indicator.</param>
        /// <param name="thresholds">The thresholds.</param>
        /// <param name="direction">The trade rule direction.</param>
        private void Initialize(dynamic indicator, int[] thresholds, Direction direction)
        {
            _thresholds = thresholds;
            Indicator = indicator;
            indicator.Updated += new IndicatorUpdatedHandler(Indicator_Updated);
            _direction = direction;
        }

        /// <summary>
        /// Exposes a means to update underlying indicator
        /// </summary>
        /// <param name="data"></param>
        public override void Update(BaseData data)
        {
            if (Indicator.GetType().IsSubclassOf(typeof(IndicatorBase<IBaseDataBar>)))
            {
                if (data.GetType().GetInterfaces().Contains(typeof(IBaseDataBar)))
                {
                    Indicator.Update((IBaseDataBar)data);
                }
            }
            else if (Indicator.GetType().IsSubclassOf(typeof(IndicatorBase<IndicatorDataPoint>)))
            {
                Indicator.Update(new IndicatorDataPoint(data.Time, data.Value));
            }
            else
            {
                Indicator.Update(data);
            }

            base.Update(data);
        }

    }
}