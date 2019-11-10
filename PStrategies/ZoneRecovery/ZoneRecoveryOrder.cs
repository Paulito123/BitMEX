namespace PStrategies.ZoneRecovery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using BitMEX.Model;
    using BitMEX.Client;

    /// <summary>
    /// An enumeration that represents the type of order within the Zone Recovery strategy.
    /// TP  = Take Profit in Profit
    /// TL  = Take Loss
    /// REV = Reverse
    /// </summary>
    public enum ZoneRecoveryOrderType { TP, TL, REV, Cancel }
    
    /// <summary>
    /// Class that represents an order sent to the exchange. 
    /// When Qty is negative, it is a Sell order.
    /// When Qty is position, it is a Long order.
    /// </summary>
    public class ZoneRecoveryOrder
    {
        /// <summary>
        /// Unique identifier of the position, as it is know on the exchange.
        /// </summary>
        public string ClOrdId { get; set; }

        /// <summary>
        /// The Order response coming from the Exchange
        /// </summary>
        public OrderResponse ServerResponse { get; set; }

        /// <summary>
        /// The symbol of the asset.
        /// </summary>
        public string Symbol { get; set; }

        /// <summary>
        /// The identifier of the account on which the position is known.
        /// </summary>
        public long Account { get; set; }

        /// <summary>
        /// The average price of the position.
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// The total quantity of the position.
        /// </summary>
        public long Qty { get; set; }

        /// <summary>
        /// The type of order.
        /// </summary>
        public ZoneRecoveryOrderType OrderType { get; set; }

        /// <summary>
        /// Constructor of the class ZoneRecoveryOrder
        /// </summary>
        /// <param name="clOrdId"></param>
        /// <param name="account"></param>
        /// <param name="price"></param>
        /// <param name="qty"></param>
        /// <param name="orderType"></param>
        public ZoneRecoveryOrder(string clOrdId, string symbol, long account, double price, long qty, ZoneRecoveryOrderType orderType)
        {
            ClOrdId = clOrdId;
            Symbol = symbol;
            Account = account;
            Price = price;
            Qty = qty;
            OrderType = orderType;
        }

        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("class ZoneRecoveryOrder {\n");
            sb.Append("  ClOrdId: ").Append(ClOrdId.ToString()).Append("\n");
            sb.Append("  Symbol: ").Append(Symbol.ToString()).Append("\n");
            sb.Append("  Account: ").Append(Account.ToString()).Append("\n");
            sb.Append("  Price: ").Append(Price.ToString()).Append("\n");
            sb.Append("  Qty: ").Append(Qty.ToString()).Append("\n");
            sb.Append("  OrderType: ").Append(OrderType.ToString()).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        public object SendOrderToServer(MordoR conn)
        {
            if (Account != conn.Account)
                return null;

            switch(OrderType)
            {
                case ZoneRecoveryOrderType.TP: // Limit order > Tegenovergestelde van open position
                    return conn.LimitOrder(Symbol, ClOrdId, Qty, Price);
                case ZoneRecoveryOrderType.TL: // Speciaal order
                    return conn.StopMarketOrder(Symbol, ClOrdId, Qty, Price, "TL");
                case ZoneRecoveryOrderType.REV:// Speciaal order
                    return conn.StopMarketOrder(Symbol, ClOrdId, Qty, Price, "REV");
                case ZoneRecoveryOrderType.Cancel:
                    var o = conn.CancelOrders(new string[] { ClOrdId });
                    if (o is List<OrderResponse>)
                        return ((List<OrderResponse>)o).First();
                    else if (o is BaseError)
                        return o;
                    else
                        return null;
                default:
                    return null;
            }
        }

        //public ZoneRecoveryOrder()
        //{

        //}
    }
}
