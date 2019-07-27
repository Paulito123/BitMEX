namespace BitMEX.Model
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class OrderResponse
    {
        [JsonProperty("orderID")]
        public string OrderId { get; set; }

        [JsonProperty("clOrdID", NullValueHandling = NullValueHandling.Ignore)]
        public string ClOrdId { get; set; }

        [JsonProperty("clOrdLinkID", NullValueHandling = NullValueHandling.Ignore)]
        public string ClOrdLinkId { get; set; }

        [JsonProperty("account", NullValueHandling = NullValueHandling.Ignore)]
        public long? Account { get; set; }

        [JsonProperty("symbol", NullValueHandling = NullValueHandling.Ignore)]
        public string Symbol { get; set; }

        [JsonProperty("side", NullValueHandling = NullValueHandling.Ignore)]
        public string Side { get; set; }

        [JsonProperty("simpleOrderQty", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic SimpleOrderQty { get; set; }

        [JsonProperty("orderQty", NullValueHandling = NullValueHandling.Ignore)]
        public long? OrderQty { get; set; }

        [JsonProperty("price", NullValueHandling = NullValueHandling.Ignore)]
        public double? Price { get; set; }

        [JsonProperty("displayQty", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic DisplayQty { get; set; }

        [JsonProperty("stopPx", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic StopPx { get; set; }

        [JsonProperty("pegOffsetValue", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic PegOffsetValue { get; set; }

        [JsonProperty("pegPriceType", NullValueHandling = NullValueHandling.Ignore)]
        public string PegPriceType { get; set; }

        [JsonProperty("currency", NullValueHandling = NullValueHandling.Ignore)]
        public string Currency { get; set; }

        [JsonProperty("settlCurrency", NullValueHandling = NullValueHandling.Ignore)]
        public string SettlCurrency { get; set; }

        [JsonProperty("ordType", NullValueHandling = NullValueHandling.Ignore)]
        public string OrdType { get; set; }

        [JsonProperty("timeInForce", NullValueHandling = NullValueHandling.Ignore)]
        public string TimeInForce { get; set; }

        [JsonProperty("execInst", NullValueHandling = NullValueHandling.Ignore)]
        public string ExecInst { get; set; }

        [JsonProperty("contingencyType", NullValueHandling = NullValueHandling.Ignore)]
        public string ContingencyType { get; set; }

        [JsonProperty("exDestination", NullValueHandling = NullValueHandling.Ignore)]
        public string ExDestination { get; set; }

        [JsonProperty("ordStatus", NullValueHandling = NullValueHandling.Ignore)]
        public string OrdStatus { get; set; }

        [JsonProperty("triggered", NullValueHandling = NullValueHandling.Ignore)]
        public string Triggered { get; set; }

        [JsonProperty("workingIndicator", NullValueHandling = NullValueHandling.Ignore)]
        public bool? WorkingIndicator { get; set; }

        [JsonProperty("ordRejReason", NullValueHandling = NullValueHandling.Ignore)]
        public string OrdRejReason { get; set; }

        [JsonProperty("simpleLeavesQty", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic SimpleLeavesQty { get; set; }

        [JsonProperty("leavesQty", NullValueHandling = NullValueHandling.Ignore)]
        public long? LeavesQty { get; set; }

        [JsonProperty("simpleCumQty", NullValueHandling = NullValueHandling.Ignore)]
        public dynamic SimpleCumQty { get; set; }

        [JsonProperty("cumQty", NullValueHandling = NullValueHandling.Ignore)]
        public long? CumQty { get; set; }

        [JsonProperty("avgPx", NullValueHandling = NullValueHandling.Ignore)]
        public double? AvgPx { get; set; }

        [JsonProperty("multiLegReportingType", NullValueHandling = NullValueHandling.Ignore)]
        public string MultiLegReportingType { get; set; }

        [JsonProperty("text", NullValueHandling = NullValueHandling.Ignore)]
        public string Text { get; set; }

        [JsonProperty("transactTime", NullValueHandling = NullValueHandling.Ignore)]
        public DateTimeOffset? TransactTime { get; set; }

        [JsonProperty("timestamp")]
        public DateTimeOffset? Timestamp { get; set; }
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
