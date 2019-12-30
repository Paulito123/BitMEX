using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyTron.ResponseHandlers
{
    class ErrorStatsHandler
    {
        private long ErrorCount;
        private long Reconnections;
        private long Disconnections;
        
        public ErrorStatsHandler()
        {
            ErrorCount = 0;
            Reconnections = 0;
            Disconnections = 0;
        }

        public void Add2ErrorCnt(long errCnt)
        {
            ErrorCount = ErrorCount + errCnt;
        }

        public void Add2Reconnections(long errCnt)
        {
            Reconnections = Reconnections + errCnt;
        }

        public void Add2Disconnections(long errCnt)
        {
            Disconnections = Disconnections + errCnt;
        }

        public long GetErrorCnt()
        {
            return ErrorCount;
        }

        public long GetReconnections()
        {
            return Reconnections;
        }

        public long GetDisconnections()
        {
            return Disconnections;
        }
    }
}
