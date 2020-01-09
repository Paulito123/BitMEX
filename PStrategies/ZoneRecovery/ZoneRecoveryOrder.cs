using System.Collections.Generic;
using System.Linq;
using Bitmex.Client.Websocket.Responses.Orders;
using BitMEXRest.Model;
using BitMEXRest.Dto;

namespace PStrategies.ZoneRecovery
{
    /// <summary>
    /// ZoneRecoveryOrder is a wrapper that contains the information about an order that is sent to an excange.
    /// </summary>
    public class ZoneRecoveryOrder
    {
        private long BatchNr { get; }
        public ZoneRecoveryAccount ZRAccount { get; }
        public OrderPOSTRequestParams PostParams { get; }
        public OrderDto LastResponse { get; set; }
        
        public ZoneRecoveryOrder(long batchNr, ZoneRecoveryAccount zra, OrderPOSTRequestParams postParams)
        {
            ZRAccount = zra;
            PostParams = postParams;
            BatchNr = batchNr;
        }
    }
}
