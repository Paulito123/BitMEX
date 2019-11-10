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
    public partial class PositionResponse
    {
        /// <summary>
        /// Account mnemonic as agreed between buy and sell sides, e.g. broker and institution or investor/intermediary and fund 
        /// manager.
        /// Account <1> field
        /// </summary>
        [JsonProperty("account")]
        public long Account { get; set; }

        /// <summary>
        /// Ticker symbol. Common, "human understood" representation of the security. SecurityID (48) value can be specified if no symbol exists (e.g. non-exchange traded Collective Investment Vehicles)
        /// Use "[N/A]" for products which do not have a symbol.
        /// Symbol <55> field
        /// </summary>
        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("underlying", NullValueHandling = NullValueHandling.Ignore)]
        public string Underlying { get; set; }

        [JsonProperty("quoteCurrency", NullValueHandling = NullValueHandling.Ignore)]
        public string QuoteCurrency { get; set; }

        [JsonProperty("commission", NullValueHandling = NullValueHandling.Ignore)]
        public double? Commission { get; set; }

        [JsonProperty("initMarginReq", NullValueHandling = NullValueHandling.Ignore)]
        public double? InitMarginReq { get; set; }

        [JsonProperty("maintMarginReq", NullValueHandling = NullValueHandling.Ignore)]
        public double? MaintMarginReq { get; set; }

        [JsonProperty("riskLimit", NullValueHandling = NullValueHandling.Ignore)]
        public long? RiskLimit { get; set; }

        /// <summary>
        /// The active leverage on a position for a specific currency.
        /// </summary>
        [JsonProperty("leverage", NullValueHandling = NullValueHandling.Ignore)]
        public long? Leverage { get; set; }

        [JsonProperty("crossMargin", NullValueHandling = NullValueHandling.Ignore)]
        public bool? CrossMargin { get; set; }

        [JsonProperty("deleveragePercentile")]
        public dynamic DeleveragePercentile { get; set; }

        [JsonProperty("rebalancedPnl", NullValueHandling = NullValueHandling.Ignore)]
        public long? RebalancedPnl { get; set; }

        [JsonProperty("prevRealisedPnl", NullValueHandling = NullValueHandling.Ignore)]
        public long? PrevRealisedPnl { get; set; }

        [JsonProperty("prevUnrealisedPnl", NullValueHandling = NullValueHandling.Ignore)]
        public long? PrevUnrealisedPnl { get; set; }

        [JsonProperty("prevClosePrice", NullValueHandling = NullValueHandling.Ignore)]
        public double? PrevClosePrice { get; set; }

        [JsonProperty("openingTimestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? OpeningTimestamp { get; set; }

        /// <summary>
        /// The very first quantity of the first order that made the current position.
        /// </summary>
        [JsonProperty("openingQty", NullValueHandling = NullValueHandling.Ignore)]
        public long? OpeningQty { get; set; }

        [JsonProperty("openingCost", NullValueHandling = NullValueHandling.Ignore)]
        public long? OpeningCost { get; set; }

        [JsonProperty("openingComm", NullValueHandling = NullValueHandling.Ignore)]
        public long? OpeningComm { get; set; }

        /// <summary>
        /// The sum of all quantities of all the resting buy orders for the respective currency. 
        /// </summary>
        [JsonProperty("openOrderBuyQty", NullValueHandling = NullValueHandling.Ignore)]
        public long? OpenOrderBuyQty { get; set; }

        /// <summary>
        /// The sum of the cost of all the resting buy orders for the respective currency. 
        /// </summary>
        [JsonProperty("openOrderBuyCost", NullValueHandling = NullValueHandling.Ignore)]
        public long? OpenOrderBuyCost { get; set; }

        /// <summary>
        /// The sum of the premiums of all the resting buy orders for the respective currency. 
        /// </summary>
        [JsonProperty("openOrderBuyPremium", NullValueHandling = NullValueHandling.Ignore)]
        public long? OpenOrderBuyPremium { get; set; }

        /// <summary>
        /// The sum of all quantities of all the resting sell orders for the respective currency. 
        /// </summary>
        [JsonProperty("openOrderSellQty", NullValueHandling = NullValueHandling.Ignore)]
        public long? OpenOrderSellQty { get; set; }

        /// <summary>
        /// The sum of the cost of all the resting sell orders for the respective currency. 
        /// </summary>
        [JsonProperty("openOrderSellCost", NullValueHandling = NullValueHandling.Ignore)]
        public long? OpenOrderSellCost { get; set; }

        /// <summary>
        /// The sum of the premiums of all the resting sell orders for the respective currency. 
        /// </summary>
        [JsonProperty("openOrderSellPremium", NullValueHandling = NullValueHandling.Ignore)]
        public long? OpenOrderSellPremium { get; set; }

        /// <summary>
        /// The last buy order quantity that has been added to the current position.
        /// </summary>
        [JsonProperty("execBuyQty", NullValueHandling = NullValueHandling.Ignore)]
        public long? ExecBuyQty { get; set; }

        /// <summary>
        /// The cost of the last buy order that has been added to the current position.
        /// </summary>
        [JsonProperty("execBuyCost", NullValueHandling = NullValueHandling.Ignore)]
        public long? ExecBuyCost { get; set; }

        /// <summary>
        /// The last sell order quantity that has been added to the current position.
        /// </summary>
        [JsonProperty("execSellQty", NullValueHandling = NullValueHandling.Ignore)]
        public long? ExecSellQty { get; set; }

        /// <summary>
        /// The cost of the last sell order that has been added to the current position.
        /// </summary>
        [JsonProperty("execSellCost", NullValueHandling = NullValueHandling.Ignore)]
        public long? ExecSellCost { get; set; }

        /// <summary>
        /// The last order quantity that has been added to the current position.
        /// </summary>
        [JsonProperty("execQty", NullValueHandling = NullValueHandling.Ignore)]
        public long? ExecQty { get; set; }

        /// <summary>
        /// The cost of the last order that has been added to the current position.
        /// </summary>
        [JsonProperty("execCost", NullValueHandling = NullValueHandling.Ignore)]
        public long? ExecCost { get; set; }

        [JsonProperty("execComm", NullValueHandling = NullValueHandling.Ignore)]
        public long? ExecComm { get; set; }

        [JsonProperty("currentTimestamp", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? CurrentTimestamp { get; set; }

        /// <summary>
        /// The total quantity of the current position. 
        /// When currentQty = 0, the entire position for the given currency is closed. At this point we are not in the market.
        /// </summary>
        [JsonProperty("currentQty")]
        public long CurrentQty { get; set; }

        /// <summary>
        /// The total cost of the current position.
        /// </summary>
        [JsonProperty("currentCost", NullValueHandling = NullValueHandling.Ignore)]
        public long CurrentCost { get; set; }

        /// <summary>
        /// The total commission of the current position.
        /// </summary>
        [JsonProperty("currentComm", NullValueHandling = NullValueHandling.Ignore)]
        public long? CurrentComm { get; set; }

        [JsonProperty("realisedCost", NullValueHandling = NullValueHandling.Ignore)]
        public long? RealisedCost { get; set; }

        [JsonProperty("unrealisedCost", NullValueHandling = NullValueHandling.Ignore)]
        public long? UnrealisedCost { get; set; }

        [JsonProperty("grossOpenCost", NullValueHandling = NullValueHandling.Ignore)]
        public long? GrossOpenCost { get; set; }

        [JsonProperty("grossOpenPremium", NullValueHandling = NullValueHandling.Ignore)]
        public long? GrossOpenPremium { get; set; }

        [JsonProperty("grossExecCost", NullValueHandling = NullValueHandling.Ignore)]
        public long? GrossExecCost { get; set; }

        /// <summary>
        /// Reflects whether the current position is in the market or not. 
        /// When isOpen = false, the currentQty = 0.
        /// </summary>
        [JsonProperty("isOpen", NullValueHandling = NullValueHandling.Ignore)]
        public bool IsOpen { get; set; }

        /// <summary>
        /// The weighted average price of the current open position. When the currentQty is 0, the markPrice is null.
        /// </summary>
        [JsonProperty("markPrice", NullValueHandling = NullValueHandling.Ignore)]
        public double MarkPrice { get; set; }

        [JsonProperty("markValue", NullValueHandling = NullValueHandling.Ignore)]
        public long? MarkValue { get; set; }

        [JsonProperty("riskValue", NullValueHandling = NullValueHandling.Ignore)]
        public long? RiskValue { get; set; }

        [JsonProperty("homeNotional", NullValueHandling = NullValueHandling.Ignore)]
        public long? HomeNotional { get; set; }

        [JsonProperty("foreignNotional", NullValueHandling = NullValueHandling.Ignore)]
        public long? ForeignNotional { get; set; }

        [JsonProperty("posState", NullValueHandling = NullValueHandling.Ignore)]
        public string PosState { get; set; }

        [JsonProperty("posCost", NullValueHandling = NullValueHandling.Ignore)]
        public long? PosCost { get; set; }

        [JsonProperty("posCost2", NullValueHandling = NullValueHandling.Ignore)]
        public long? PosCost2 { get; set; }

        [JsonProperty("posCross", NullValueHandling = NullValueHandling.Ignore)]
        public long? PosCross { get; set; }

        [JsonProperty("posInit", NullValueHandling = NullValueHandling.Ignore)]
        public long? PosInit { get; set; }

        [JsonProperty("posComm", NullValueHandling = NullValueHandling.Ignore)]
        public long? PosComm { get; set; }

        [JsonProperty("posLoss", NullValueHandling = NullValueHandling.Ignore)]
        public long? PosLoss { get; set; }

        [JsonProperty("posMargin", NullValueHandling = NullValueHandling.Ignore)]
        public long? PosMargin { get; set; }

        [JsonProperty("posMaint", NullValueHandling = NullValueHandling.Ignore)]
        public long? PosMaint { get; set; }

        [JsonProperty("posAllowance", NullValueHandling = NullValueHandling.Ignore)]
        public long? PosAllowance { get; set; }

        [JsonProperty("taxableMargin", NullValueHandling = NullValueHandling.Ignore)]
        public long? TaxableMargin { get; set; }

        [JsonProperty("initMargin", NullValueHandling = NullValueHandling.Ignore)]
        public long? InitMargin { get; set; }

        [JsonProperty("maintMargin", NullValueHandling = NullValueHandling.Ignore)]
        public long? MaintMargin { get; set; }

        [JsonProperty("sessionMargin", NullValueHandling = NullValueHandling.Ignore)]
        public long? SessionMargin { get; set; }

        [JsonProperty("targetExcessMargin", NullValueHandling = NullValueHandling.Ignore)]
        public long? TargetExcessMargin { get; set; }

        [JsonProperty("varMargin", NullValueHandling = NullValueHandling.Ignore)]
        public long? VarMargin { get; set; }

        [JsonProperty("realisedGrossPnl", NullValueHandling = NullValueHandling.Ignore)]
        public long? RealisedGrossPnl { get; set; }

        [JsonProperty("realisedTax", NullValueHandling = NullValueHandling.Ignore)]
        public long? RealisedTax { get; set; }

        [JsonProperty("realisedPnl", NullValueHandling = NullValueHandling.Ignore)]
        public long? RealisedPnl { get; set; }

        [JsonProperty("unrealisedGrossPnl", NullValueHandling = NullValueHandling.Ignore)]
        public long? UnrealisedGrossPnl { get; set; }

        [JsonProperty("longBankrupt", NullValueHandling = NullValueHandling.Ignore)]
        public long? LongBankrupt { get; set; }

        [JsonProperty("shortBankrupt", NullValueHandling = NullValueHandling.Ignore)]
        public long? ShortBankrupt { get; set; }

        [JsonProperty("taxBase", NullValueHandling = NullValueHandling.Ignore)]
        public long? TaxBase { get; set; }

        [JsonProperty("indicativeTaxRate", NullValueHandling = NullValueHandling.Ignore)]
        public long? IndicativeTaxRate { get; set; }

        [JsonProperty("indicativeTax", NullValueHandling = NullValueHandling.Ignore)]
        public long? IndicativeTax { get; set; }

        [JsonProperty("unrealisedTax", NullValueHandling = NullValueHandling.Ignore)]
        public long? UnrealisedTax { get; set; }

        [JsonProperty("unrealisedPnl", NullValueHandling = NullValueHandling.Ignore)]
        public long? UnrealisedPnl { get; set; }

        [JsonProperty("unrealisedPnlPcnt", NullValueHandling = NullValueHandling.Ignore)]
        public long? UnrealisedPnlPcnt { get; set; }

        [JsonProperty("unrealisedRoePcnt", NullValueHandling = NullValueHandling.Ignore)]
        public long? UnrealisedRoePcnt { get; set; }

        [JsonProperty("simpleQty", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic SimpleQty { get; set; }

        [JsonProperty("simpleCost", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic SimpleCost { get; set; }

        [JsonProperty("simpleValue", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic SimpleValue { get; set; }

        [JsonProperty("simplePnl", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic SimplePnl { get; set; }

        [JsonProperty("simplePnlPcnt", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic SimplePnlPcnt { get; set; }

        [JsonProperty("avgCostPrice", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic AvgCostPrice { get; set; }

        [JsonProperty("avgEntryPrice", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic AvgEntryPrice { get; set; }

        [JsonProperty("breakEvenPrice", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic BreakEvenPrice { get; set; }

        [JsonProperty("marginCallPrice", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic MarginCallPrice { get; set; }

        [JsonProperty("liquidationPrice")]
        public dynamic LiquidationPrice { get; set; }

        [JsonProperty("bankruptPrice", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic BankruptPrice { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset? Timestamp { get; set; }

        [JsonProperty("lastPrice", NullValueHandling = NullValueHandling.Ignore)]
        public double? LastPrice { get; set; }

        [JsonProperty("lastValue", NullValueHandling = NullValueHandling.Ignore)]
        public long? LastValue { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("class PositionResponse {\n");
            sb.Append("  Account: ").Append(Account.ToString()).Append("\n");
            sb.Append("  Symbol: ").Append(Symbol.ToString()).Append("\n");
            sb.Append("  Currency: ").Append(Currency.ToString()).Append("\n");
            sb.Append("  Underlying: ").Append(Underlying.ToString()).Append("\n");
            sb.Append("  QuoteCurrency: ").Append(QuoteCurrency.ToString()).Append("\n");
            sb.Append("  Commission: ").Append(Commission.ToString()).Append("\n");
            sb.Append("  InitMarginReq: ").Append(InitMarginReq.ToString()).Append("\n");
            sb.Append("  MaintMarginReq: ").Append(MaintMarginReq.ToString()).Append("\n");
            sb.Append("  RiskLimit: ").Append(RiskLimit.ToString()).Append("\n");
            sb.Append("  Leverage: ").Append(Leverage.ToString()).Append("\n");
            sb.Append("  CrossMargin: ").Append(CrossMargin.ToString()).Append("\n");
            //sb.Append("  DeleveragePercentile: ").Append(DeleveragePercentile.ToString()).Append("\n");
            sb.Append("  RebalancedPnl: ").Append(RebalancedPnl.ToString()).Append("\n");
            sb.Append("  PrevRealisedPnl: ").Append(PrevRealisedPnl.ToString()).Append("\n");
            sb.Append("  PrevUnrealisedPnl: ").Append(PrevUnrealisedPnl.ToString()).Append("\n");
            sb.Append("  PrevClosePrice: ").Append(PrevClosePrice.ToString()).Append("\n");
            sb.Append("  OpeningTimestamp: ").Append(OpeningTimestamp.ToString()).Append("\n");
            sb.Append("  OpeningQty: ").Append(OpeningQty.ToString()).Append("\n");
            sb.Append("  OpeningCost: ").Append(OpeningCost.ToString()).Append("\n");
            sb.Append("  OpeningComm: ").Append(OpeningComm.ToString()).Append("\n");
            sb.Append("  OpenOrderBuyQty: ").Append(OpenOrderBuyQty.ToString()).Append("\n");
            sb.Append("  OpenOrderBuyCost: ").Append(OpenOrderBuyCost.ToString()).Append("\n");
            sb.Append("  OpenOrderBuyPremium: ").Append(OpenOrderBuyPremium.ToString()).Append("\n");
            sb.Append("  OpenOrderSellQty: ").Append(OpenOrderSellQty.ToString()).Append("\n");
            sb.Append("  OpenOrderSellCost: ").Append(OpenOrderSellCost.ToString()).Append("\n");
            sb.Append("  OpenOrderSellPremium: ").Append(OpenOrderSellPremium.ToString()).Append("\n");
            sb.Append("  ExecBuyQty: ").Append(ExecBuyQty.ToString()).Append("\n");
            sb.Append("  ExecBuyCost: ").Append(ExecBuyCost.ToString()).Append("\n");
            sb.Append("  ExecSellQty: ").Append(ExecSellQty.ToString()).Append("\n");
            sb.Append("  ExecSellCost: ").Append(ExecSellCost.ToString()).Append("\n");
            sb.Append("  ExecQty: ").Append(ExecQty.ToString()).Append("\n");
            sb.Append("  ExecCost: ").Append(ExecCost.ToString()).Append("\n");
            sb.Append("  ExecComm: ").Append(ExecComm.ToString()).Append("\n");
            sb.Append("  CurrentTimestamp: ").Append(CurrentTimestamp.ToString()).Append("\n");
            sb.Append("  CurrentQty: ").Append(CurrentQty.ToString()).Append("\n");
            sb.Append("  CurrentCost: ").Append(CurrentCost.ToString()).Append("\n");
            sb.Append("  CurrentComm: ").Append(CurrentComm.ToString()).Append("\n");
            sb.Append("  RealisedCost: ").Append(RealisedCost.ToString()).Append("\n");
            sb.Append("  UnrealisedCost: ").Append(UnrealisedCost.ToString()).Append("\n");
            sb.Append("  GrossOpenCost: ").Append(GrossOpenCost.ToString()).Append("\n");
            sb.Append("  GrossOpenPremium: ").Append(GrossOpenPremium.ToString()).Append("\n");
            sb.Append("  GrossExecCost: ").Append(GrossExecCost.ToString()).Append("\n");
            sb.Append("  IsOpen: ").Append(IsOpen.ToString()).Append("\n");
            sb.Append("  MarkPrice: ").Append(MarkPrice.ToString()).Append("\n");
            sb.Append("  MarkValue: ").Append(MarkValue.ToString()).Append("\n");
            sb.Append("  RiskValue: ").Append(RiskValue.ToString()).Append("\n");
            sb.Append("  HomeNotional: ").Append(HomeNotional.ToString()).Append("\n");
            sb.Append("  ForeignNotional: ").Append(ForeignNotional.ToString()).Append("\n");
            sb.Append("  PosState: ").Append(PosState.ToString()).Append("\n");
            sb.Append("  PosCost: ").Append(PosCost.ToString()).Append("\n");
            sb.Append("  PosCost2: ").Append(PosCost2.ToString()).Append("\n");
            sb.Append("  PosCross: ").Append(PosCross.ToString()).Append("\n");
            sb.Append("  PosInit: ").Append(PosInit.ToString()).Append("\n");
            sb.Append("  PosComm: ").Append(PosComm.ToString()).Append("\n");
            sb.Append("  PosLoss: ").Append(PosLoss.ToString()).Append("\n");
            sb.Append("  PosMargin: ").Append(PosMargin.ToString()).Append("\n");
            sb.Append("  PosMaint: ").Append(PosMaint.ToString()).Append("\n");
            sb.Append("  PosAllowance: ").Append(PosAllowance.ToString()).Append("\n");
            sb.Append("  TaxableMargin: ").Append(TaxableMargin.ToString()).Append("\n");
            sb.Append("  InitMargin: ").Append(InitMargin.ToString()).Append("\n");
            sb.Append("  MaintMargin: ").Append(MaintMargin.ToString()).Append("\n");
            sb.Append("  SessionMargin: ").Append(SessionMargin.ToString()).Append("\n");
            sb.Append("  TargetExcessMargin: ").Append(TargetExcessMargin.ToString()).Append("\n");
            sb.Append("  VarMargin: ").Append(VarMargin.ToString()).Append("\n");
            sb.Append("  RealisedGrossPnl: ").Append(RealisedGrossPnl.ToString()).Append("\n");
            sb.Append("  RealisedTax: ").Append(RealisedTax.ToString()).Append("\n");
            sb.Append("  RealisedPnl: ").Append(RealisedPnl.ToString()).Append("\n");
            sb.Append("  UnrealisedGrossPnl: ").Append(UnrealisedGrossPnl.ToString()).Append("\n");
            sb.Append("  LongBankrupt: ").Append(LongBankrupt.ToString()).Append("\n");
            sb.Append("  ShortBankrupt: ").Append(ShortBankrupt.ToString()).Append("\n");
            sb.Append("  TaxBase: ").Append(TaxBase.ToString()).Append("\n");
            sb.Append("  IndicativeTaxRate: ").Append(IndicativeTaxRate.ToString()).Append("\n");
            sb.Append("  IndicativeTax: ").Append(IndicativeTax.ToString()).Append("\n");
            sb.Append("  UnrealisedTax: ").Append(UnrealisedTax.ToString()).Append("\n");
            sb.Append("  UnrealisedPnl: ").Append(UnrealisedPnl.ToString()).Append("\n");
            sb.Append("  UnrealisedPnlPcnt: ").Append(UnrealisedPnlPcnt.ToString()).Append("\n");
            sb.Append("  UnrealisedRoePcnt: ").Append(UnrealisedRoePcnt.ToString()).Append("\n");
            //sb.Append("  SimpleQty: ").Append(SimpleQty.ToString()).Append("\n");
            //sb.Append("  SimpleCost: ").Append(SimpleCost.ToString()).Append("\n");
            //sb.Append("  SimpleValue: ").Append(SimpleValue.ToString()).Append("\n");
            //sb.Append("  SimplePnl: ").Append(SimplePnl.ToString()).Append("\n");
            //sb.Append("  SimplePnlPcnt: ").Append(SimplePnlPcnt.ToString()).Append("\n");
            //sb.Append("  AvgCostPrice: ").Append(AvgCostPrice.ToString()).Append("\n");
            //sb.Append("  AvgEntryPrice: ").Append(AvgEntryPrice.ToString()).Append("\n");
            //sb.Append("  BreakEvenPrice: ").Append(BreakEvenPrice.ToString()).Append("\n");
            //sb.Append("  MarginCallPrice: ").Append(MarginCallPrice.ToString()).Append("\n");
            //sb.Append("  LiquidationPrice: ").Append(LiquidationPrice.ToString()).Append("\n");
            //sb.Append("  BankruptPrice: ").Append(BankruptPrice.ToString()).Append("\n");
            sb.Append("  Timestamp: ").Append(Timestamp.ToString()).Append("\n");
            sb.Append("  LastPrice: ").Append(LastPrice.ToString()).Append("\n");
            sb.Append("  LastValue: ").Append(LastValue.ToString()).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }

    public partial class PositionResponse
    {
        public static List<PositionResponse> FromJson(string json)
        {
            return JsonConvert.DeserializeObject<List<PositionResponse>>(json, PositionConverter.Settings);
        }
    }

    public partial class PositionsResponse
    {
        public static List<PositionResponse> FromJson(string json)
        {
            return JsonConvert.DeserializeObject<List<PositionResponse>>(json, PositionConverter.Settings);
        }
    }

    internal static class PositionConverter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
