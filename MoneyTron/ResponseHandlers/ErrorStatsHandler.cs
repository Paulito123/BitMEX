using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PStrategies.ZoneRecovery;

namespace MoneyTron.ResponseHandlers
{
    class ErrorStatsHandler
    {
        private readonly Dictionary<ZoneRecoveryAccount, int> ErrorCount = new Dictionary<ZoneRecoveryAccount, int>();
        private readonly Dictionary<ZoneRecoveryAccount, int> Reconnections = new Dictionary<ZoneRecoveryAccount, int>();
        private readonly Dictionary<ZoneRecoveryAccount, int> Disconnections = new Dictionary<ZoneRecoveryAccount, int>();
        
        public ErrorStatsHandler()
        {
            ErrorCount.Add(ZoneRecoveryAccount.A, 0);
            ErrorCount.Add(ZoneRecoveryAccount.B, 0);
            Reconnections.Add(ZoneRecoveryAccount.A, 0);
            Reconnections.Add(ZoneRecoveryAccount.B, 0);
            Disconnections.Add(ZoneRecoveryAccount.A, 0);
            Disconnections.Add(ZoneRecoveryAccount.B, 0);
        }

        public void Add2ErrorCnt(int errCnt, ZoneRecoveryAccount acc)
        {
            ErrorCount[acc] = ErrorCount[acc] + errCnt;
        }

        public void Add2Reconnections(int errCnt, ZoneRecoveryAccount acc)
        {
            Reconnections[acc] = Reconnections[acc] + errCnt;
        }

        public void Add2Disconnections(int errCnt, ZoneRecoveryAccount acc)
        {
            Disconnections[acc] = Disconnections[acc] + errCnt;
        }

        public int GetErrorCnt(ZoneRecoveryAccount acc)
        {
            return ErrorCount[acc];
        }

        public int GetReconnections(ZoneRecoveryAccount acc)
        {
            return Reconnections[acc];
        }

        public int GetDisconnections(ZoneRecoveryAccount acc)
        {
            return Disconnections[acc];
        }
    }
}
