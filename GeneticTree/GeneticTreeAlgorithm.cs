









































































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

        
    }
}













































