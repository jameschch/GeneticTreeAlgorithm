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
    public partial class GeneticTreeAlgorithmExample : QCAlgorithm
    {
        private Rule _entry;
        private Rule _exit;
        private Symbol _symbol;
        private readonly bool IsOutOfSampleRun = true;
        private readonly int oosPeriod = 6;

        public override void Initialize()
        {
            SetCash(1000);
            SetStartDate(Config.GetValue<DateTime>("startDate", new DateTime(2017, 6, 12)));
            SetEndDate(Config.GetValue<DateTime>("endDate", new DateTime(2017, 7, 22)));

            if (IsOutOfSampleRun)
            {
                var startDate = new DateTime(year: 2016, month: 1, day: 1);
                SetStartDate(startDate);
                SetEndDate(startDate.AddMonths(oosPeriod));
                RuntimeStatistics["ID"] = GetParameter("ID");
                SetParameters(config.ToDictionary(k => k.Key, v => v.Value.ToString()));
            }

            _symbol = AddSecurity(SecurityType.Crypto, "BTCUSD", Resolution.Tick, Market.GDAX, false, 1m, false).Symbol;
            SetBrokerageModel(QuantConnect.Brokerages.BrokerageName.GDAX, AccountType.Cash);
            var con = new TickConsolidator(new TimeSpan(1, 0, 0));

            SetBenchmark(_symbol);

            var factory = new SignalFactory(7);

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
            if (!Portfolio.Invested && _entry.IsTrue())
            {
                SetHoldings(_symbol, 0.9m);
                Log("buy: " + Portfolio[_symbol].Price + " Portfolio:" + Portfolio.TotalPortfolioValue);
            }
            else if (_exit.IsTrue())
            {
                Liquidate();
                Log("liq: " + Portfolio[_symbol].Price + " Portfolio:" + Portfolio.TotalPortfolioValue);
            }
        }

        private static Dictionary<string, int> config = new Dictionary<string, int> {
            {"EntryIndicator1",  0},
            {"EntryIndicator2",  1},
            {"EntryIndicator3",  -1},
            {"EntryIndicator4",  2},
            {"EntryIndicator5",  3},
            {"EntryIndicator6",  4},
            {"EntryIndicator7",  5},
            {"EntryIndicator1Direction",  0},
            {"EntryIndicator2Direction",  0},
            {"EntryIndicator3Direction",  1},
            {"EntryIndicator4Direction",  0},
            {"EntryIndicator5Direction",  1},
            {"EntryIndicator6Direction",  1},
            {"EntryIndicator7Direction",  1},
            {"EntryOperator1",  0},
            {"EntryOperator2",  1},
            {"EntryOperator3",  0},
            {"EntryOperator4",  0},
            {"EntryOperator5",  0},
            {"EntryOperator6",  0},
            {"ExitIndicator1",  6},
            {"ExitIndicator2",  7},
            {"ExitIndicator3",  8},
            {"ExitIndicator4",  9},
            {"ExitIndicator5",  10},
            {"ExitIndicator6",  11},
            {"ExitIndicator7",  12},
            {"ExitIndicator1Direction",  0},
            {"ExitIndicator2Direction",  0},
            {"ExitIndicator3Direction",  1},
            {"ExitIndicator4Direction",  1},
            {"ExitIndicator5Direction",  0},
            {"ExitIndicator6Direction",  0},
            {"ExitIndicator7Direction",  0},
            {"ExitOperator1",  0},
            {"ExitOperator2",  0},
            {"ExitOperator3",  0},
            {"ExitOperator4",  1},
            {"ExitOperator5",  0},
            {"ExitOperator6",  0},
            {"period",  1},
            {"slowPeriod",  2},
            {"fastPeriod",  3},
            {"signalPeriod",  4 }
        };
    }
}













































