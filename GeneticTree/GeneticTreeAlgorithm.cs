using System;
using QuantConnect.Data;
using QuantConnect.Configuration;
using QuantConnect.Data.Consolidators;
using System.Collections.Generic;
using System.Linq;
using QuantConnect.Algorithm;
using QuantConnect;

namespace GeneticTree
{
    public partial class GeneticTreeAlgorithm : QCAlgorithm
    {
        private Rule _entry;
        private Rule _exit;
        private Symbol _symbol;
        private readonly bool IsOutOfSampleRun = false;
        private readonly int oosPeriod = 3;

        public override void Initialize()
        {
            SetCash(1000);
            SetStartDate(Config.GetValue<DateTime>("startDate", new DateTime(2017, 6, 12)));
            SetEndDate(Config.GetValue<DateTime>("endDate", new DateTime(2017, 7, 22)));

            if (IsOutOfSampleRun)
            {
                var startDate = new DateTime(year: 2016, month: 1, day: 1);
                SetEndDate(startDate.AddMonths(oosPeriod));
                SetStartDate(startDate);
                RuntimeStatistics["ID"] = GetParameter("ID");
            }

            _symbol = AddSecurity(SecurityType.Forex, "BTCUSD", Resolution.Tick, Market.GDAX, false, 3.3m, false).Symbol;
            SetBrokerageModel(QuantConnect.Brokerages.BrokerageName.GDAX);
            //Securities["BTCUSD"].FeeModel = new MakerTakerFeeModel();
            var con = new TickConsolidator(new TimeSpan(1, 0, 0));
            SubscriptionManager.AddConsolidator(_symbol, con);
            SetBenchmark(_symbol);

            //SetParameters(parametersToBacktest.ToDictionary(k => k.Key, v => v.Value.ToString()));

            var factory = new SignalFactory();

            _entry = factory.Create(this, _symbol, true);
            _exit = factory.Create(this, _symbol, false);
        }

        public override void OnData(Slice e)
        {
            if (!LiveMode && Portfolio.TotalPortfolioValue < 600)
            {
                Quit();
            }

            if (!_entry.IsReady()) return;
            if (!Portfolio.Invested)
            {
                if (_entry.IsTrue())
                {
                    SetHoldings(_symbol, percentage: 3m);
                    Log("buy: " + Portfolio[_symbol].Price + " Portfolio:" + Portfolio.TotalPortfolioValue);
                };
            }
            else
            {
                if (_exit.IsTrue())
                {
                    Liquidate(_symbol);
                    Log("liq: " + Portfolio[_symbol].Price + " Portfolio:" + Portfolio.TotalPortfolioValue);
                }
            }
        }

        /// <summary>
        ///     Here are the parameters of the individual with the best in-sample fitness.
        /// </summary>
        private readonly Dictionary<string, int> parametersToBacktest = new Dictionary<string, int>
        {
                {"EntryIndicator1",  0},
                {"EntryIndicator2",  1},
                {"EntryIndicator3",  6},
                {"EntryIndicator4",  5},
                {"EntryIndicator5",  -1},
                {"EntryIndicator1Direction",  0},
                {"EntryIndicator2Direction",  0},
                {"EntryIndicator3Direction",  1},
                {"EntryIndicator4Direction",  0},
                {"EntryIndicator5Direction",  1},
                {"EntryOperator1",  0},
                {"EntryOperator2",  1},
                {"EntryOperator3",  0},
                {"EntryOperator4",  0},
                {"EntryRelationship1",  0},
                {"EntryRelationship2",  1},
                {"EntryRelationship3",  1},
                {"EntryRelationship4",  1},
                {"EntryRelationship5",  0},
                {"ExitIndicator1",  7},
                {"ExitIndicator2",  6},
                {"ExitIndicator3",  1},
                {"ExitIndicator4",  5},
                {"ExitIndicator5",  0},
                {"ExitIndicator1Direction",  0},
                {"ExitIndicator2Direction",  0},
                {"ExitIndicator3Direction",  1},
                {"ExitIndicator4Direction",  1},
                {"ExitIndicator5Direction",  0},
                {"ExitOperator1",  0},
                {"ExitOperator2",  0},
                {"ExitOperator3",  0},
                {"ExitOperator4",  1},
                {"ExitRelationship1",  0},
                {"ExitRelationship2",  1},
                {"ExitRelationship3",  0},
                {"ExitRelationship4",  1},
                {"period",  9},
                {"slowPeriod",  453},
                {"fastPeriod",  40},
                {"signalPeriod",  18 }
        };
    }
}