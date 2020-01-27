using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Orders;

using BitMEXRest.Client;
using BitMEXRest.Dto;
using BitMEXRest.Model;

using Serilog;

namespace PStrategies.ZoneRecovery.State
{
    abstract class ZoneRecoveryState
    {
        protected Calculator calculator;
        protected int zRPosition;
        protected ZoneRecoveryStatus currentStatus;

        public Calculator Calculator
        {
            get => calculator;
            set => calculator = value;
        }

        public int ZRPosition
        {
            get => zRPosition;
            set => zRPosition = value;
        }

        public ZoneRecoveryStatus CurrentStatus
        {
            get => currentStatus;
            set => currentStatus = value;
        }

        public abstract void Evaluate();
    }


}
