using System;
using System.ComponentModel;
using Newtonsoft.Json;

namespace BitMEXRest.Model
{
    /// <summary>
    /// To get open orders only, send {"open": true} in the filter param. e.g. new Dictionary _string,string_(){{"open","true"}}
    /// </summary>
    public partial class OrderGETRequestParams : QueryStringParamsWithFilter
    {
        /// <summary>
        /// Instrument symbol. Send a bare series (e.g. XBU) to get data for the nearest expiring contract in that series.
        /// You can also send a timeframe, e.g.XBU:monthly.Timeframes are daily, weekly, monthly, quarterly, and biquarterly
        /// </summary>
        [DisplayName("symbol")]
        public string Symbol { get; set; }
        /// <summary>
        ///  Array of column names to fetch. If omitted, will return all columns.Note that this method will always return item keys, even when not specified, so you may receive more columns that you expect.
        /// </summary>
        [DisplayName("columns")]
        public string Columns { get; set; }
        /// <summary>
        /// Number of results to fetch.
        /// </summary>
        [DisplayName("count")]
        public decimal? Count { get; set; }
        /// <summary>
        /// Starting point for results.
        /// </summary>
        [DisplayName("start")]
        public decimal? Start { get; set; }
        /// <summary>
        /// If true, will sort results newest first.
        /// </summary>
        [DisplayName("reverse")]
        public bool Reverse { get; set; }
        /// <summary>
        /// Starting date filter for results
        /// </summary>
        [DisplayName("startTime")]
        public DateTime? StartTime { get; set; }
        /// <summary>
        /// Ending date filter for results.
        /// </summary>
        [DisplayName("endTime")]
        public DateTime? EndTime { get; set; }
    }
    /// <summary>
    /// Send an orderID or origClOrdID to identify the order you wish to amend.
    /// Both order quantity and price can be amended.Only one qty field can be used to amend.
    /// Use the leavesQty field to specify how much of the order you wish to remain open.
    /// This can be useful if you want to adjust your position's delta by a certain amount, regardless of how much of the order has already filled.
    /// A leavesQty can be used to make a "Filled" order live again, if it is received within 60 seconds of the fill.
    /// Use the simpleOrderQty and simpleLeavesQty fields to specify order size in Bitcoin, rather than contracts.These fields will round up to the nearest contract.
    /// Like order placement, amending can be done in bulk.Simply send a request to PUT /api/v1/order/bulk with a JSON body of the shape: { "orders": [{...}, {...}]}, each object containing the fields used in this endpoint.
    /// </summary>
    public partial class OrderPUTRequestParams : QueryJsonParams
    {
        /// <summary>
        /// Order ID
        /// </summary>
        [JsonProperty("orderID")]
        public string OrderID { get; set; }
        /// <summary>
        /// Client Order ID.See POST /order.
        /// </summary>
        [JsonProperty("origClOrdID")]
        public string OrigClOrdID { get; set; }
        /// <summary>
        /// Optional new Client Order ID, requires origClOrdID.
        /// </summary>
        [JsonProperty("clOrdID")]
        public string ClOrdID { get; set; }
        /// <summary>
        /// Optional order quantity in units of the instrument (i.e. contracts).
        /// </summary>
        [JsonProperty("orderQty")]
        public decimal? OrderQty { get; set; }

        [JsonProperty("displayQty")]
        public decimal? DisplayQty { get; set; }
        /// <summary>
        /// Optional leaves quantity in units of the instrument (i.e. contracts). Useful for amending partially filled orders.
        /// </summary>
        [JsonProperty("leavesQty")]
        public decimal? LeavesQty { get; set; }
        /// <summary>
        /// Optional limit price for 'Limit', 'StopLimit', and 'LimitIfTouched' orders.
        /// </summary>
        [JsonProperty("price")]
        public decimal? Price { get; set; }
        /// <summary>
        /// Optional trigger price for 'Stop', 'StopLimit', 'MarketIfTouched', and 'LimitIfTouched' orders.Use a price below the current price for stop-sell orders and buy-if-touched orders.
        /// </summary>
        [JsonProperty("stopPx")]
        public decimal? StopPx { get; set; }
        /// <summary>
        /// Optional trailing offset from the current price for 'Stop', 'StopLimit', 'MarketIfTouched', and 'LimitIfTouched' orders; 
        /// use a negative offset for stop-sell orders and buy-if-touched orders. Optional offset from the peg price for 'Pegged' orders.
        /// </summary>
        [JsonProperty("pegOffsetValue")]
        public decimal? PegOffsetValue { get; set; }
        /// <summary>
        /// Optional amend annotation. e.g. 'Adjust skew'. 42
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }
    }
    /// <summary>
    /// https://testnet.bitmex.com/api/explorer/#!/Order/Order_new
    /// </summary>
    public partial class OrderPOSTRequestParams : QueryJsonParams
    {
        /// <summary>
        /// Instrument symbol. e.g. 'XBTUSD'.
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        /// <summary>
        /// Order side.Valid options: Buy, Sell.Defaults to 'Buy' unless orderQty or simpleOrderQty is negative.
        /// </summary>
        [JsonProperty("side")]
        public string Side { get; set; }
        /// <summary>
        /// Order quantity in units of the instrument (i.e. contracts).
        /// </summary>
        [JsonProperty("orderQty")]
        public decimal? OrderQty { get; set; }
        /// <summary>
        /// Optional limit price for 'Limit', 'StopLimit', and 'LimitIfTouched' orders.
        /// </summary>
        [JsonProperty("price")]
        public decimal? Price { get; set; }
        /// <summary>
        /// 
        /// </summary>
        [JsonProperty("displayQty")]
        public decimal? DisplayQty { get; set; }
        /// <summary>
        /// Optional quantity to display in the book. Use 0 for a fully hidden order.
        /// </summary>
        [JsonProperty("stopPx")]
        public decimal? StopPx { get; set; }
        /// <summary>
        /// Optional trigger price for 'Stop', 'StopLimit', 'MarketIfTouched', and 'LimitIfTouched' orders. 
        /// Use a price below the current price for stop-sell orders and buy-if-touched orders.
        /// Use execInst of 'MarkPrice' or 'LastPrice' to define the current price used for triggering.
        /// </summary>
        [JsonProperty("clOrdID")]
        public string ClOrdID { get; set; }
        /// <summary>
        /// Optional Client Order ID. This clOrdID will come back on the order and any related executions.
        /// </summary>
        [JsonProperty("clOrdLinkID")]
        public string ClOrdLinkID { get; set; }
        /// <summary>
        /// Optional trailing offset from the current price for 'Stop', 'StopLimit', 'MarketIfTouched', and 'LimitIfTouched' orders;
        /// use a negative offset for stop-sell orders and buy-if-touched orders. Optional offset from the peg price for 'Pegged' orders.
        /// </summary>
        [JsonProperty("pegOffsetValue")]
        public decimal? PegOffsetValue { get; set; }
        /// <summary>
        /// Optional peg price type. Valid options: LastPeg, MidPricePeg, MarketPeg, PrimaryPeg, TrailingStopPeg.
        /// </summary>
        [JsonProperty("pegPriceType")]
        public string PegPriceType { get; set; }
        /// <summary>
        /// Order type. Valid options: Market, Limit, Stop, StopLimit, MarketIfTouched, LimitIfTouched, MarketWithLeftOverAsLimit, Pegged. 
        /// Defaults to 'Limit' when price is specified. Defaults to 'Stop' when stopPx is specified. Defaults to 'StopLimit' when price and stopPx are specified.
        /// </summary>
        [JsonProperty("ordType")]
        public string OrdType { get; set; }
        /// <summary>
        /// Time in force. Valid options: Day, GoodTillCancel, ImmediateOrCancel, FillOrKill.
        /// Defaults to 'GoodTillCancel' for 'Limit', 'StopLimit', 'LimitIfTouched', and 'MarketWithLeftOverAsLimit' orders.
        /// </summary>
        [JsonProperty("timeInForce")]
        public string TimeInForce { get; set; }
        /// <summary>
        /// Optional execution instructions.Valid options: ParticipateDoNotInitiate, AllOrNone, MarkPrice, IndexPrice, LastPrice, Close, ReduceOnly, Fixed.
        /// 'AllOrNone' instruction requires displayQty to be 0. 'MarkPrice', 'IndexPrice' or 'LastPrice' instruction valid for 'Stop', 'StopLimit', 'MarketIfTouched', 
        /// and 'LimitIfTouched' orders.
        /// </summary>
        [JsonProperty("execInst")]
        public string ExecInst { get; set; }
        /// <summary>
        /// Optional order annotation. e.g. 'Take profit on MOON!'.
        /// </summary>
        [JsonProperty("text")]
        public string Text { get; set; }
    }
    public partial class OrderDELETERequestParams : QueryStringParamsWithFilter
    {
        /// <summary>
        /// Order ID(s).
        /// </summary>
        [DisplayName("orderID")]
        public string OrderID { get; set; }
        /// <summary>
        /// Client Order ID(s). See POST /order.
        /// </summary>
        [DisplayName("clOrdID")]
        public string ClOrdID { get; set; }
        /// <summary>
        /// Optional cancellation annotation. e.g. 'Spread Exceeded'.
        /// </summary>
        [DisplayName("text")]
        public string Text { get; set; }
    }
    /// <summary>
    /// Delete all orders
    /// </summary>
    public partial class OrderAllDELETERequestParams : QueryStringParamsWithFilter
    {
        /// <summary>
        ///  Optional symbol.If provided, only cancels orders for that symbol.
        /// </summary>
        [DisplayName("symbol")]
        public string Symbol { get; set; }
        /// <summary>
        /// Optional filter for cancellation. Use to only cancel some orders, e.g. {"side": "Buy"}.
        /// </summary>
        [DisplayName("filter")]
        public string Filter { get; set; }
        /// <summary>        
        /// Optional cancellation annotation.e.g. 'Spread Exceeded'
        /// </summary>
        [DisplayName("text")]
        public string Text { get; set; }
    }
    /// <summary>
    /// Amend multiple orders for the same symbol.
    /// </summary>
    public partial class OrderBulkPUTRequestParams : QueryJsonParams
    {
        /// <summary>
        /// List of orders to amend
        /// </summary>
        [JsonProperty("orders")]
        public OrderPUTRequestParams[] Orders { get; set; }
    }
    /// <summary>
    /// Create multiple new orders for the same symbol.
    /// </summary>
    public partial class OrderBulkPOSTRequestParams : QueryJsonParams
    {
        /// <summary>
        /// List of orders to create
        /// </summary>
        [JsonProperty("orders")]
        public OrderPOSTRequestParams[] Orders { get; set; }
    }
    /// <summary>
    /// Automatically cancel all your orders after a specified timeout.
    /// Useful as a dead-man's switch to ensure your orders are canceled in case of an outage. If called repeatedly, the existing offset will be canceled and a new one will be inserted in its place.
    /// Example usage: call this route at 15s intervals with an offset of 60000 (60s). If this route is not called within 60 seconds, all your orders will be automatically canceled.
    /// </summary>
    public partial class OrderCancelAllAfterPOSTRequestParams : QueryJsonParams
    {
        /// <summary>
        /// Timeout in ms. Set to 0 to cancel this timer
        /// </summary>
        [JsonProperty("timeout")]
        public int Timeout { get; set; }
    }
    /// <summary>
    /// Close a position. [Deprecated, use POST /order with execInst: 'Close'
    /// </summary>
    [Obsolete]
    public partial class OrderClosePositionPOSTRequestParams : QueryJsonParams
    {
        /// <summary>
        /// Symbol of position to close.
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }
        /// <summary>
        /// Optional limit price.
        /// </summary>
        [JsonProperty("price")]
        public decimal? Price { get; set; }
    }
}
