using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PStrategies.ZoneRecovery.State;

namespace PStrategies.ZoneRecovery
{
    public class CalculatorStats
    {
        public string Symbol;
        public int MaxDepthIndex;
        public decimal ZoneSize;
        public decimal MaxExposurePerc;
        public decimal Leverage;
        public decimal MinimumProfitPercentage;
        public long RunningBatchNr;
        public long UnitSize;
        public decimal WalletBalance;

        public ZoneRecoveryDirection Direction;
        internal ZoneRecoveryState State;
        
    }
}
