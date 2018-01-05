using System;
using QuantConnect.Indicators;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using System.Linq;

namespace GeneticTree.Signal
{

    /// <summary>
    ///     This class keeps track if prices are moving towards a direction by enlarging donchian channel, 
    /// if prices breaks channel for N consecutive periods, then trigger signal
    /// </summary>
    /// <seealso cref="QuantConnect.Algorithm.CSharp.ITechnicalIndicatorSignal" />
    public class DonchianSignal : SignalBase
    {

        protected Direction _direction;
        private IndicatorBase<IndicatorDataPoint> _upperBand;
        private IndicatorBase<IndicatorDataPoint> _lowerBand;
        private DonchianChannel _donchian;

        /// <summary>
        ///     Initializes a new instance of the <see cref="DonchianSignal" /> class.
        /// </summary>
        /// <param name="indicator">The indicator.</param>
        /// <param name="periods">The periods.</param>
        /// <param name="direction">
        ///     The trade rule direction. Only used if the instance will be part of a
        ///     <see cref="Rule" /> class
        /// </param>
        /// <remarks>The oscillator must be registered BEFORE being used by this constructor.</remarks>
        public DonchianSignal(dynamic indicator, DonchianChannel donchian, int periods, Direction direction = Direction.LongOnly)
        {
            Initialize(indicator, direction);
            //donchian.UpperBand.Updated += new IndicatorUpdatedHandler(Max_Updated);
            //donchian.LowerBand.Updated += new IndicatorUpdatedHandler(Min_Updated);
            _donchian = donchian;
            _upperBand = donchian.UpperBand; 
            _lowerBand = donchian.LowerBand;
            SurvivalWindow = new RollingWindow<int>(periods);
            indicator.Updated += new IndicatorUpdatedHandler(Price_Updated);
        }
        /// <summary>
        ///     The underlying indicator, must be an oscillator.
        /// </summary>
        public dynamic Indicator { get; private set; }

        /// <summary>
        ///     Gets a value indicating whether this instance is ready.
        /// </summary>
        /// <value>
        ///     <c>true</c> if this instance is ready; otherwise, <c>false</c>.
        /// </value>
        public override bool IsReady
        {
            get { return Indicator.IsReady && _donchian.IsReady; }
        }

        public override decimal Value
        {
            get { return SurvivalWindow[0]; }
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
                        signal = SurvivalWindow[0] == 1;
                        for (int i = 1; signal && i < SurvivalWindow.Count; i++)
                        {
                            if (SurvivalWindow[0] != SurvivalWindow[i])
                            {
                                signal = false;
                            }
                        }
                        break;

                    case Direction.ShortOnly:
                        signal = SurvivalWindow[0] == -1;
                        for (int i = 1; signal && i < SurvivalWindow.Count; i++)
                        {
                            if (SurvivalWindow[0] != SurvivalWindow[i])
                            {
                                signal = false;
                            }
                        }
                        break;
                }
            }
            return signal;
        }


        /// <summary>
        /// Sets up class.
        /// </summary>
        /// <param name="indicator">The indicator.</param>
        /// <param name="direction">The trade rule direction.</param>
        private void Initialize(dynamic indicator, Direction direction)
        {
            Indicator = indicator;
            _direction = direction;
        }

        private void Max_Updated(object sender, IndicatorDataPoint updated)
        {
            if (IsReady)
            {
                if (((IndicatorBase<IndicatorDataPoint>)Indicator).Current.Value == updated.Value)
                {
                    SurvivalWindow.Add(1);
                }
            }
        }

        private void Min_Updated(object sender, IndicatorDataPoint updated)
        {
            if (IsReady)
            {
                if (((IndicatorBase<IndicatorDataPoint>)Indicator).Current.Value == updated.Value)
                {
                    SurvivalWindow.Add(-1);
                }
            }
        }

        private void Price_Updated(object sender, IndicatorDataPoint updated)
        {
            if (IsReady)
            {
                if (updated.Value >= _upperBand.Current.Value )
                {
                    SurvivalWindow.Add(1);
                }
                else if (updated.Value <= _lowerBand.Current.Value)
                {
                    SurvivalWindow.Add(-1);
                }
                else
                {
                    SurvivalWindow.Add(0);
                }
            }
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