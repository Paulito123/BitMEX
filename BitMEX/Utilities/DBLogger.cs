using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitMEX.JSONClass.Order;

namespace BitMEX.Utilities
{
    public enum LogType { ltOrderRequest, ltOrderResponse, ltError };

    public class DBLogger
    {
        public LogType logType { get; protected set; }
        public object logObject { get; protected set; }


    }
}
