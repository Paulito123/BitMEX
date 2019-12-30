using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System;
using System.Reflection;
using Bitmex.Client.Websocket.Responses.Margins;
using Serilog;

namespace MoneyTron.ResponseHandlers
{
    class MarginStatsHandler
    {
        private long WalletBalance = 0;
        private long MarginBalance = 0;
        private long AvailableMargin = 0;

        public void UpdateBalances(long? walletBalance = null, long? marginBalance = null, long? availableBalance = null)
        {
            if (walletBalance != null && walletBalance != 0)
                WalletBalance = (long)walletBalance;
            if (marginBalance != null && marginBalance != 0)
                MarginBalance = (long)marginBalance;
            if (availableBalance != null && availableBalance != 0)
                AvailableMargin = (long)availableBalance;
        }

        public MarginStats GetMarginBalances()
        {
            return new MarginStats(WalletBalance, MarginBalance, AvailableMargin);
        }
    }

    class MarginStats
    {
        public static readonly MarginStats NULL = new MarginStats(0, 0, 0);

        public long WalletBalance { get; }
        public long MarginBalance { get; }
        public long AvailableMargin { get; }

        public MarginStats(long walletBalance, long marginBalance, long availableBalance)
        {
            WalletBalance = walletBalance;
            MarginBalance = marginBalance;
            AvailableMargin = availableBalance;
        }
    }
}
