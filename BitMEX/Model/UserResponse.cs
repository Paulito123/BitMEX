namespace BitMEX.Model
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class UserResponse
    {
        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("ownerId")]
        public long OwnerId { get; set; }

        [JsonProperty("firstname")]
        public string Firstname { get; set; }

        [JsonProperty("lastname")]
        public string Lastname { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("phone")]
        public string Phone { get; set; }

        [JsonProperty("created")]
        public DateTimeOffset Created { get; set; }

        [JsonProperty("lastUpdated")]
        public DateTimeOffset LastUpdated { get; set; }

        [JsonProperty("preferences")]
        public Preferences Preferences { get; set; }

        [JsonProperty("restrictedEngineFields")]
        public RestrictedEngineFields RestrictedEngineFields { get; set; }

        [JsonProperty("TFAEnabled")]
        public string TfaEnabled { get; set; }

        [JsonProperty("affiliateID")]
        public string AffiliateId { get; set; }

        [JsonProperty("pgpPubKey")]
        public string PgpPubKey { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("geoipCountry")]
        public string GeoipCountry { get; set; }

        [JsonProperty("geoipRegion")]
        public string GeoipRegion { get; set; }

        [JsonProperty("typ")]
        public string Typ { get; set; }
    }

    public partial class Preferences
    {
        [JsonProperty("alertOnLiquidations")]
        public bool AlertOnLiquidations { get; set; }

        [JsonProperty("animationsEnabled")]
        public bool AnimationsEnabled { get; set; }

        [JsonProperty("announcementsLastSeen")]
        public DateTimeOffset AnnouncementsLastSeen { get; set; }

        [JsonProperty("chatChannelID")]
        public long ChatChannelId { get; set; }

        [JsonProperty("colorTheme")]
        public string ColorTheme { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("debug")]
        public bool Debug { get; set; }

        [JsonProperty("disableEmails")]
        public List<string> DisableEmails { get; set; }

        [JsonProperty("disablePush")]
        public List<string> DisablePush { get; set; }

        [JsonProperty("hideConfirmDialogs")]
        public List<string> HideConfirmDialogs { get; set; }

        [JsonProperty("hideConnectionModal")]
        public bool HideConnectionModal { get; set; }

        [JsonProperty("hideFromLeaderboard")]
        public bool HideFromLeaderboard { get; set; }

        [JsonProperty("hideNameFromLeaderboard")]
        public bool HideNameFromLeaderboard { get; set; }

        [JsonProperty("hideNotifications")]
        public List<string> HideNotifications { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("msgsSeen")]
        public List<string> MsgsSeen { get; set; }

        [JsonProperty("orderBookBinning")]
        public RestrictedEngineFields OrderBookBinning { get; set; }

        [JsonProperty("orderBookType")]
        public string OrderBookType { get; set; }

        [JsonProperty("orderClearImmediate")]
        public bool OrderClearImmediate { get; set; }

        [JsonProperty("orderControlsPlusMinus")]
        public bool OrderControlsPlusMinus { get; set; }

        [JsonProperty("showLocaleNumbers")]
        public bool ShowLocaleNumbers { get; set; }

        [JsonProperty("sounds")]
        public List<string> Sounds { get; set; }

        [JsonProperty("strictIPCheck")]
        public bool StrictIpCheck { get; set; }

        [JsonProperty("strictTimeout")]
        public bool StrictTimeout { get; set; }

        [JsonProperty("tickerGroup")]
        public string TickerGroup { get; set; }

        [JsonProperty("tickerPinned")]
        public bool TickerPinned { get; set; }

        [JsonProperty("tradeLayout")]
        public string TradeLayout { get; set; }
    }

    public partial class RestrictedEngineFields
    {
    }

    public partial class UserResponse
    {
        public static UserResponse FromJson(string json)
        {
            return JsonConvert.DeserializeObject<UserResponse>(json, UserConverter.Settings);
        }
    }

    public static class Serialize
    {
        public static string ToJson(this UserResponse self)
        {
            return JsonConvert.SerializeObject(self, UserConverter.Settings);
        }
    }

    internal static class UserConverter
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
