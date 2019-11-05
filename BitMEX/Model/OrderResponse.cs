namespace BitMEX.Model
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    /// <summary>
    /// Info on all the fields:
    ///     https://www.onixs.biz/fix-dictionary/5.0.SP2/fields_by_name.html
    /// </summary>
    public partial class OrderResponse
    {
        /// <summary>
        /// Unique identifier for Order as assigned by sell-side (broker, exchange, ECN). Uniqueness must be guaranteed within a single 
        /// trading day. Firms which accept multi-day orders should consider embedding a date within the OrderID field to assure 
        /// uniqueness across days.
        /// OrderID <37> field
        /// </summary>
        [JsonProperty("orderID")]
        public string OrderId { get; set; }

        /// <summary>
        /// Unique identifier for Order as assigned by the buy-side (institution, broker, intermediary etc.) (identified by 
        /// SenderCompID (49) or OnBehalfOfCompID (5) as appropriate). Uniqueness must be guaranteed within a single trading day. Firms, 
        /// particularly those which electronically submit multi-day orders, trade globally or throughout market close periods, should 
        /// ensure uniqueness across days, for example by embedding a date within the ClOrdID field.
        /// ClOrdID <11> field
        /// </summary>
        [JsonProperty("clOrdID", NullValueHandling = NullValueHandling.Ignore)]
        public string ClOrdId { get; set; }

        /// <summary>
        /// DEPRECATED
        /// Permits order originators to tie together groups of orders in which trades resulting from orders are associated for a 
        /// specific purpose, for example the calculation of average execution price for a customer or to associate lists submitted to 
        /// a broker as waves of a larger program trade.
        /// ClOrdLinkID <583> field
        /// </summary>
        [JsonProperty("clOrdLinkID", NullValueHandling = NullValueHandling.Ignore)]
        public string ClOrdLinkId { get; set; }

        /// <summary>
        /// Account mnemonic as agreed between buy and sell sides, e.g. broker and institution or investor/intermediary and fund 
        /// manager.
        /// Account <1> field
        /// </summary>
        [JsonProperty("account", NullValueHandling = NullValueHandling.Ignore)]
        public long Account { get; set; }

        /// <summary>
        /// Ticker symbol. Common, "human understood" representation of the security. SecurityID (48) value can be specified if no symbol exists (e.g. non-exchange traded Collective Investment Vehicles)
        /// Use "[N/A]" for products which do not have a symbol.
        /// Symbol <55> field
        /// </summary>
        [JsonProperty("symbol", NullValueHandling = NullValueHandling.Ignore)]
        public string Symbol { get; set; }

        /// <summary>
        /// Side of order (see Volume : "Glossary" for value definitions)
        /// Valid values:
        /// 1 = Buy
        /// 2 = Sell
        /// Below are not used, i think...
        /// 3 = Buy minus
        /// 4 = Sell plus
        /// 5 = Sell short
        /// 6 = Sell short exempt
        /// 7 = Undisclosed(valid for IOI and List Order messages only)
        /// 8 = Cross(orders where counterparty is an exchange, valid for all messages except IOIs)
        /// 9 = Cross short
        /// A = Cross short exempt
        /// B = "As Defined" (for use with multileg instruments)
        /// C = "Opposite" (for use with multileg instruments)
        /// D = Subscribe(e.g.CIV)
        /// E = Redeem(e.g.CIV)
        /// F = Lend(FINANCING - identifies direction of collateral)
        /// G = Borrow(FINANCING - identifies direction of collateral)
        /// Side <54> field
        /// </summary>
        [JsonProperty("side", NullValueHandling = NullValueHandling.Ignore)]
        public string Side { get; set; }

        /// <summary>
        /// DEPRECATED
        /// No description
        /// </summary>
        [JsonProperty("simpleOrderQty", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic SimpleOrderQty { get; set; }

        /// <summary>
        /// Quantity ordered. This represents the number of shares for equities or par, face or nominal value for FI instruments.
        /// OrderQty <38> field
        /// </summary>
        [JsonProperty("orderQty", NullValueHandling = NullValueHandling.Ignore)]
        public long? OrderQty { get; set; }

        /// <summary>
        /// Price per unit of quantity (e.g. per share)
        /// Price <44> field
        /// </summary>
        [JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]
        public double? Price { get; set; }

        /// <summary>
        /// The quantity to be displayed . Required for reserve orders. On orders specifies the qty to be displayed, on execution reports 
        /// the currently displayed quantity.
        /// DisplayQty <1138> field
        /// </summary>
        [JsonProperty("displayQty", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic DisplayQty { get; set; }

        /// <summary>
        /// Price per unit of quantity (e.g. per share)
        /// StopPx <99> field
        /// </summary>
        [JsonProperty("stopPx", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic StopPx { get; set; }

        /// <summary>
        /// Amount (signed) added to the peg for a pegged order in the context of the PegOffsetType (836)
        /// PegOffsetValue <211> field
        /// </summary>
        [JsonProperty("pegOffsetValue", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic PegOffsetValue { get; set; }

        /// <summary>
        /// Defines the type of peg.
        /// Valid values:
        /// 1 = Last peg(last sale)
        /// 2 = Mid-price peg(midprice of inside quote)
        /// 3 = Opening peg
        /// 4 = Market peg
        /// 5 = Primary peg(primary market - buy at bid or sell at offer)
        /// 7 = Peg to VWAP
        /// 8 = Trailing Stop Peg
        /// 9 = Peg to Limit Price
        /// PegPriceType <1094> field
        /// </summary>
        [JsonProperty("pegPriceType", NullValueHandling = NullValueHandling.Ignore)]
        public string PegPriceType { get; set; }

        /// <summary>
        /// Identifies currency used for price. Absence of this field is interpreted as the default for the security. It is recommended 
        /// that systems provide the currency value whenever possible. See "Appendix 6-A: Valid Currency Codes" for information on 
        /// obtaining valid values.
        /// Currency <15> field
        /// </summary>
        [JsonProperty("currency", NullValueHandling = NullValueHandling.Ignore)]
        public string Currency { get; set; }

        /// <summary>
        /// Currency code of settlement denomination.
        /// SettlCurrency <120> field
        /// </summary>
        [JsonProperty("settlCurrency", NullValueHandling = NullValueHandling.Ignore)]
        public string SettlCurrency { get; set; }

        /// <summary>
        /// Valid values:
        /// 1 = Market
        /// 2 = Limit
        /// 3 = Stop / Stop Loss
        /// 4 = Stop Limit
        /// 5 = Market On Close(No longer used)
        /// 6 = With Or Without
        /// 7 = Limit Or Better
        /// 8 = Limit With Or Without
        /// 9 = On Basis
        /// A = On Close(No longer used)
        /// B = Limit On Close(No longer used)
        /// C = Forex Market(No longer used)
        /// D = Previously Quoted
        /// E = Previously Indicated
        /// F = Forex Limit(No longer used)
        /// G = Forex Swap
        /// H = Forex Previously Quoted(No longer used)
        /// I = Funari(Limit day order with unexecuted portion handles as Market On Close.E.g.Japan)
        /// J = Market If Touched(MIT)
        /// K = Market With Left Over as Limit(market order with unexecuted quantity becoming limit order at last price)
        /// L = Previous Fund Valuation Point(Historic pricing; for CIV)
        /// M = Next Fund Valuation Point(Forward pricing; for CIV)
        /// P = Pegged
        /// Q = Counter - order selection
        /// OrdType <40> field
        /// </summary>
        [JsonProperty("ordType", NullValueHandling = NullValueHandling.Ignore)]
        public string OrdType { get; set; }

        /// <summary>
        /// Specifies how long the order remains in effect. Absence of this field is interpreted as DAY. NOTE not applicable to CIV Orders. (see Volume : "Glossary" for value definitions)
        /// Valid values:
        /// 0 = Day(or session)
        /// 1 = Good Till Cancel(GTC)
        /// 2 = At the Opening(OPG)
        /// 3 = Immediate Or Cancel(IOC)
        /// 4 = Fill Or Kill(FOK)
        /// 5 = Good Till Crossing(GTX)
        /// 6 = Good Till Date(GTD)
        /// 7 = At the Close
        /// 8 = Good Through Crossing
        /// 9 = At Crossing
        /// TimeInForce <59> field
        /// </summary>
        [JsonProperty("timeInForce", NullValueHandling = NullValueHandling.Ignore)]
        public string TimeInForce { get; set; }

        /// <summary>
        /// Instructions for order handling on exchange trading floor. If more than one instruction is applicable to an order, this field can contain multiple instructions separated by space. *** SOME VALUES HAVE BEEN REPLACED - See "Replaced Features and Supported Approach" *** (see Volume : "Glossary" for value definitions)
        /// Valid values:
        /// 0 = Stay on offer side
        /// 1 = Not held
        /// 2 = Work
        /// 3 = Go along
        /// 4 = Over the day
        /// 5 = Held
        /// 6 = Participant don't initiate
        /// 7 = Strict scale
        /// 8 = Try to scale
        /// 9 = Stay on bid side
        /// A = No cross(cross is forbidden)
        /// a = Trailing Stop Peg
        /// B = OK to cross
        /// b = Strict Limit(No price improvement)
        /// c = Ignore Price Validity Checks
        /// C = Call first
        /// d = Peg to Limit Price
        /// D = Percent of volume(indicates that the sender does not want to be all of the volume on the floor vs.a specific percentage)
        /// E = Do not increase - DNI
        /// e = Work to Target Strategy
        /// F = Do not reduce - DNR
        /// G = All or none - AON
        /// H = Reinstate on system failure(mutually exclusive with Q and l)
        /// I = Institutions only
        /// J = Reinstate on Trading Halt(mutually exclusive with K and m)
        /// K = Cancel on Trading Halt(mutually exclusive with J and m)
        /// L = Last peg(last sale)
        /// M = Mid-price peg(midprice of inside quote)
        /// N = Non-negotiable
        /// O = Opening peg
        /// P = Market peg
        /// Q = Cancel on system failure(mutually exclusive with H and l)
        /// R = Primary peg(primary market - buy at bid/sell at offer)
        /// S = Suspend
        /// U = Customer Display Instruction(Rule 11Ac1-1/4)
        /// V = Netting(for Forex)
        /// W = Peg to VWAP
        /// X = Trade Along
        /// Y = Try To Stop
        /// Z = Cancel if not best
        /// f = Intermarket Sweep
        /// j = Single execution requested for block trade
        /// g = External Routing Allowed
        /// h = External Routing Not Allowed
        /// i = Imbalance Only
        /// T = Fixed Peg to Local best bid or offer at time of order
        /// k = Best Execution
        /// l = Suspend on system failure(mutually exclusive with H and Q)
        /// m = Suspend on Trading Halt(mutually exclusive with J and K)
        /// n = Reinstate on connection loss(mutually exclusive with o and p)
        /// o = Cancel on connection loss(mutually exclusive with n and p)
        /// p = Suspend on connection loss(mutually exclusive with n and o)
        /// q = Release from suspension(mutually exclusive with S)
        /// r = Execute as delta neutral using volatility provided
        /// s = Execute as duration neutral
        /// t = Execute as FX neutral
        /// ExecInst <18> field
        /// </summary>
        [JsonProperty("execInst", NullValueHandling = NullValueHandling.Ignore)]
        public string ExecInst { get; set; }

        /// <summary>
        /// Defines the type of contingency.
        /// Valid values:
        /// 1 = One Cancels the Other(OCO)
        /// 2 = One Triggers the Other(OTO)
        /// 3 = One Updates the Other(OUO) - Absolute Quantity Reduction
        /// 4 = One Updates the Other(OUO) - Proportional Quantity Reduction
        /// ContingencyType <1385> field
        /// </summary>
        [JsonProperty("contingencyType", NullValueHandling = NullValueHandling.Ignore)]
        public string ContingencyType { get; set; }

        /// <summary>
        /// Execution destination as defined by institution when order is entered.
        /// ExDestination <100> field
        /// </summary>
        [JsonProperty("exDestination", NullValueHandling = NullValueHandling.Ignore)]
        public string ExDestination { get; set; }

        /// <summary>
        /// Identifies current status of order. *** SOME VALUES HAVE BEEN REPLACED - See "Replaced Features and Supported Approach" *** 
        /// (see Volume : "Glossary" for value definitions)
        /// Valid values:
        /// 0 = New
        /// 1 = Partially filled
        /// 2 = Filled
        /// 3 = Done for day
        /// 4 = Canceled
        /// 6 = Pending Cancel(i.e.result of Order Cancel Request)
        /// 7 = Stopped
        /// 8 = Rejected
        /// 9 = Suspended
        /// A = Pending New
        /// B = Calculated
        /// C = Expired
        /// D = Accepted for Bidding
        /// E = Pending Replace(i.e.result of Order Cancel/Replace Request)
        /// 5 = Replaced(No longer used)
        /// OrdStatus <39> field
        /// </summary>
        [JsonProperty("ordStatus", NullValueHandling = NullValueHandling.Ignore)]
        public string OrdStatus { get; set; }

        /// <summary>
        /// DEPRECATED
        /// No description
        /// </summary>
        [JsonProperty("triggered", NullValueHandling = NullValueHandling.Ignore)]
        public string Triggered { get; set; }

        /// <summary>
        /// Indicates if the order is currently being worked. Applicable only for OrdStatus = "New". For open outcry markets this 
        /// indicates that the order is being worked in the crowd. For electronic markets it indicates that the order has transitioned 
        /// from a contingent order to a market order.
        /// Valid values:
        /// false = Order has been accepted but not yet in a working state
        /// true = Order is currently being worked
        /// WorkingIndicator <636> field
        /// </summary>
        [JsonProperty("workingIndicator", NullValueHandling = NullValueHandling.Ignore)]
        public bool? WorkingIndicator { get; set; }

        /// <summary>
        /// Code to identify reason for order rejection. Note: Values 3, 4, and 5 will be used when rejecting an order due to 
        /// pre-allocation information errors.
        /// Valid values:
        /// 0 = Broker / Exchange option
        /// 1 = Unknown symbol
        /// 10 = Invalid Investor ID
        /// 11 = Unsupported order characteristic
        /// 12 = Surveillence Option
        /// 13 = Incorrect quantity
        /// 14 = Incorrect allocated quantity
        /// 15 = Unknown account(s)
        /// 2 = Exchange closed
        /// 3 = Order exceeds limit
        /// 4 = Too late to enter
        /// 5 = Unknown order
        /// 6 = Duplicate Order(e.g.dupe ClOrdID)
        /// 7 = Duplicate of a verbally communicated order
        /// 8 = Stale order
        /// 9 = Trade along required
        /// 99 = Other
        /// 18 = Invalid price increment
        /// 16 = Price exceeds current price band
        /// OrdRejReason <103> field
        /// </summary>
        [JsonProperty("ordRejReason", NullValueHandling = NullValueHandling.Ignore)]
        public string OrdRejReason { get; set; }

        /// <summary>
        /// DEPRECATED
        /// No description
        /// </summary>
        [JsonProperty("simpleLeavesQty", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic SimpleLeavesQty { get; set; }

        /// <summary>
        /// Quantity open for further execution. If the OrdStatus (39) is Canceled, DoneForTheDay, Expired, Calculated, or Rejected 
        /// (in which case the order is no longer active) then LeavesQty could be 0, otherwise LeavesQty = OrderQty (38) - CumQty (14).
        /// LeavesQty <151> field
        /// </summary>
        [JsonProperty("leavesQty", NullValueHandling = NullValueHandling.Ignore)]
        public long? LeavesQty { get; set; }

        /// <summary>
        /// DEPRECATED
        /// No description
        /// </summary>
        [JsonProperty("simpleCumQty", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic SimpleCumQty { get; set; }

        /// <summary>
        /// Total quantity (e.g. number of shares) filled.
        /// CumQty <14> field
        /// </summary>
        [JsonProperty("cumQty", NullValueHandling = NullValueHandling.Ignore)]
        public long? CumQty { get; set; }

        /// <summary>
        /// Calculated average price of all fills on this order.
        /// For Fixed Income trades AvgPx is always expressed as percent-of-par, regardless of the PriceType(423) of LastPx(31). 
        /// I.e., AvgPx will contain an average of percent-of-par values(see LastParPx (669)) for issues traded in Yield, Spread 
        /// or Discount.
        /// AvgPx <6> field
        /// </summary>
        [JsonProperty("avgPx", NullValueHandling = NullValueHandling.Ignore)]
        public double? AvgPx { get; set; }

        /// <summary>
        /// Used to indicate what an Execution Report represents (e.g. used with multi-leg securities, such as option strategies, spreads, etc.).
        /// Valid values:
        /// 1 = Single security(default if not specified)
        /// 2 = Individual leg of a multi-leg security
        /// 3 = Multi-leg security
        /// MultiLegReportingType <442> field
        /// </summary>
        [JsonProperty("multiLegReportingType", NullValueHandling = NullValueHandling.Ignore)]
        public string MultiLegReportingType { get; set; }

        /// <summary>
        /// Free format text string
        /// Text <58> field
        /// </summary>
        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }

        /// <summary>
        /// Timestamp when the business transaction represented by the message occurred.
        /// TransactTime <60> field
        /// </summary>
        [JsonProperty("transactTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? TransactTime { get; set; }

        /// <summary>
        /// No description
        /// </summary>
        [JsonProperty("timestamp")]
        public DateTimeOffset? Timestamp { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("class OrderResponse {\n");
            sb.Append("  OrderID: ").Append(OrderId.ToString()).Append("\n");
            sb.Append("  ClOrdID: ").Append(ClOrdId.ToString()).Append("\n");
            sb.Append("  ClOrdLinkID: ").Append(ClOrdLinkId.ToString()).Append("\n");
            sb.Append("  Account: ").Append(Account.ToString()).Append("\n");
            sb.Append("  Symbol: ").Append(Symbol.ToString()).Append("\n");
            sb.Append("  Side: ").Append(Side.ToString()).Append("\n");
            sb.Append("  OrderQty: ").Append(OrderQty.ToString()).Append("\n");
            sb.Append("  Price: ").Append(Price.ToString()).Append("\n");
            sb.Append("  DisplayQty: ").Append((DisplayQty == null) ? "null" : DisplayQty.ToString()).Append("\n");
            sb.Append("  StopPx: ").Append((StopPx == null) ? "null" : StopPx.ToString()).Append("\n");
            sb.Append("  PegOffsetValue: ").Append((PegOffsetValue == null) ? "null" : PegOffsetValue.ToString()).Append("\n");
            sb.Append("  PegPriceType: ").Append((PegPriceType == null) ? "null" : PegPriceType.ToString()).Append("\n");
            sb.Append("  Currency: ").Append((Currency == null) ? "null" : Currency.ToString()).Append("\n");
            sb.Append("  SettlCurrency: ").Append((SettlCurrency == null) ? "null" : SettlCurrency.ToString()).Append("\n");
            sb.Append("  OrdType: ").Append((OrdType == null) ? "null" : OrdType.ToString()).Append("\n");
            sb.Append("  TimeInForce: ").Append((TimeInForce == null) ? "null" : TimeInForce.ToString()).Append("\n");
            sb.Append("  ExecInst: ").Append((ExecInst == null) ? "null" : ExecInst.ToString()).Append("\n");
            sb.Append("  ContingencyType: ").Append((ContingencyType == null) ? "null" : ContingencyType.ToString()).Append("\n");
            sb.Append("  ExDestination: ").Append((ExDestination == null) ? "null" : ExDestination.ToString()).Append("\n");
            sb.Append("  OrdStatus: ").Append((OrdStatus == null) ? "null" : OrdStatus.ToString()).Append("\n");
            sb.Append("  Triggered: ").Append((Triggered == null) ? "null" : Triggered.ToString()).Append("\n");
            sb.Append("  WorkingIndicator: ").Append((WorkingIndicator == null) ? "null" : WorkingIndicator.ToString()).Append("\n");
            sb.Append("  OrdRejReason: ").Append((OrdRejReason == null) ? "null" : OrdRejReason.ToString()).Append("\n");
            sb.Append("  LeavesQty: ").Append((LeavesQty == null) ? "null" : LeavesQty.ToString()).Append("\n");
            sb.Append("  CumQty: ").Append((CumQty == null) ? "null" : CumQty.ToString()).Append("\n");
            sb.Append("  AvgPx: ").Append((AvgPx == null) ? "null" : AvgPx.ToString()).Append("\n");
            sb.Append("  MultiLegReportingType: ").Append((MultiLegReportingType == null) ? "null" : MultiLegReportingType.ToString()).Append("\n");
            sb.Append("  Text: ").Append((Text == null) ? "null" : Text.ToString()).Append("\n");
            sb.Append("  TransactTime: ").Append(TransactTime.ToString()).Append("\n");
            sb.Append("  Timestamp: ").Append(Timestamp.ToString()).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }

    public partial class OrderResponse
    {
        public static OrderResponse FromJson(string json)
        {
            return JsonConvert.DeserializeObject<OrderResponse>(json, BitMEX.Model.OrderConverter.Settings);
        }
    }

    public partial class OrdersResponse
    {
        public static List<OrderResponse> FromJson(string json)
        {
            return JsonConvert.DeserializeObject<List<OrderResponse>>(json, BitMEX.Model.OrderConverter.Settings);
        }
    }

    internal static class OrderConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            //MissingMemberHandling = MissingMemberHandling.Ignore,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
