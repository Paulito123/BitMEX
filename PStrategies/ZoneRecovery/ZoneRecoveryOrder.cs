using System.Collections.Generic;
using System.Linq;
using Bitmex.Client.Websocket.Responses.Orders;
using BitMEXRest.Model;
using BitMEXRest.Dto;
using BitMEXRest.Client;
using Serilog;

namespace PStrategies.ZoneRecovery
{
    /// <summary>
    /// ZoneRecoveryOrder is a wrapper that contains the information about an order that is sent to an exchange.
    /// </summary>
    public class ZoneRecoveryOrder
    {
        #region Core variables

        private IBitmexApiService Api { get; }
        public ZoneRecoveryOrderType ZROrderType { get; }
        public ZoneRecoveryAccount ZRAccount { get; }

        #endregion Core variables

        #region Containers

        public OrderPOSTRequestParams PostParams { get; }
        public OrderDto FirstServerResponse
        {
            get => FirstServerResponse;
            set => SetFirstServerResponse(value);
        }
        public OrderDto DeleteServerResponse
        {
            get => DeleteServerResponse;
            set => SetDeleteServerResponse(value);
        }

        #endregion Containers

        #region Variables used to define the current "state"

        public ZoneRecoveryOrderStatus CurrentStatus { get; set; } = ZoneRecoveryOrderStatus.Undefined;

        #endregion Variables used to define the current "state"

        #region Constructors

        public ZoneRecoveryOrder(ZoneRecoveryAccount zra, OrderPOSTRequestParams postParams, IBitmexApiService api, ZoneRecoveryOrderType type)
        {
            ZRAccount = zra;
            PostParams = postParams;
            Api = api;
            ZROrderType = type;
            DeleteServerResponse = new OrderDto();
            FirstServerResponse = new OrderDto();
        }

        #endregion Constructors

        #region Internal cheese

        private void SetLastOrderStatus(Order o)
        {
            CurrentStatus = GetOrderStatus(o);
        }

        private OrderDto SetFirstServerResponse(OrderDto o)
        {
            CurrentStatus = GetOrderStatus(o);
            return o;
        }

        private OrderDto SetDeleteServerResponse(OrderDto o)
        {
            CurrentStatus = GetOrderStatus(o);
            return o;
        }

        private ZoneRecoveryOrderStatus GetOrderStatus(object o)
        {
            if (o != null && o is Order)
            {
                switch (((Order)o).OrdStatus)
                {
                    case OrderStatus.Canceled:
                        return ZoneRecoveryOrderStatus.Canceled;
                    case OrderStatus.Filled:
                        return ZoneRecoveryOrderStatus.Filled;
                    case OrderStatus.New:
                        return ZoneRecoveryOrderStatus.New;
                    case OrderStatus.PartiallyFilled:
                        return ZoneRecoveryOrderStatus.PartiallyFilled;
                    case OrderStatus.Rejected:
                        return ZoneRecoveryOrderStatus.Rejected;
                }
            }
            else if (o != null && o is OrderDto)
            {
                switch (((OrderDto)o).OrdStatus)
                {
                    case "Canceled":
                        return ZoneRecoveryOrderStatus.Canceled;
                    case "Filled":
                        return ZoneRecoveryOrderStatus.Filled;
                    case "New":
                    case "New,Triggered":
                        return ZoneRecoveryOrderStatus.New;
                    case "Partially filled":
                        return ZoneRecoveryOrderStatus.PartiallyFilled;
                    case "Rejected":
                        return ZoneRecoveryOrderStatus.Rejected;
                }
            }
            else if (o == null)
            {
                return ZoneRecoveryOrderStatus.Error;
            }

            return ZoneRecoveryOrderStatus.Undefined;
        }

        #endregion Internal cheese

        #region External cheese

        public void KillMe()
        {
            var OrderParams = new OrderDELETERequestParams() { ClOrdID = PostParams.ClOrdID };
            var result = Api.Execute(BitmexApiUrls.Order.DeleteOrder, OrderParams).Result;
            DeleteServerResponse = result.Result.Where(x => x.ClOrdId == PostParams.ClOrdID).Single();
        }

        #endregion External cheese
    }

}
