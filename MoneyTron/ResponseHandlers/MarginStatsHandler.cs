using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System;
using System.Reflection;
using Bitmex.Client.Websocket.Responses.Margins;
using Serilog;
using PStrategies.ZoneRecovery;

namespace MoneyTron.ResponseHandlers
{
    class MarginStatsHandler
    {
        private readonly Dictionary<ZoneRecoveryAccount, long> WalletBalance   = new Dictionary<ZoneRecoveryAccount, long>();
        private readonly Dictionary<ZoneRecoveryAccount, long> MarginBalance   = new Dictionary<ZoneRecoveryAccount, long>();
        private readonly Dictionary<ZoneRecoveryAccount, long> AvailableMargin = new Dictionary<ZoneRecoveryAccount, long>();

        public MarginStatsHandler ()
        {
            WalletBalance.Add(ZoneRecoveryAccount.A, 0);
            WalletBalance.Add(ZoneRecoveryAccount.B, 0);
            MarginBalance.Add(ZoneRecoveryAccount.A, 0);
            MarginBalance.Add(ZoneRecoveryAccount.B, 0);
            AvailableMargin.Add(ZoneRecoveryAccount.A, 0);
            AvailableMargin.Add(ZoneRecoveryAccount.B, 0);
        }

        public void UpdateBalances(ZoneRecoveryAccount acc, long? walletBalance = null, long? marginBalance = null, long? availableBalance = null)
        {
            if (walletBalance != null)
                WalletBalance[acc] = (long)walletBalance;
            if (marginBalance != null)
                MarginBalance[acc] = (long)marginBalance;
            if (availableBalance != null)
                AvailableMargin[acc] = (long)availableBalance;
        }

        public MarginStats GetMarginBalances(ZoneRecoveryAccount acc)
        {
            return new MarginStats(WalletBalance[acc], MarginBalance[acc], AvailableMargin[acc]);
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
