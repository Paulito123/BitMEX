namespace BitMEX.Model
{
    using System;
    using System.Collections.Generic;

    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class BaseError
    {
        [JsonProperty("error")]
        public Error Error { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new System.Text.StringBuilder();
            sb.Append("class Error {\n");
            sb.Append("  Message: ").Append(Error.Message.ToString()).Append("\n");
            sb.Append("  Name: ").Append(Error.Name.ToString()).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
    }

    public partial class Error
    {
        [JsonProperty("message", NullValueHandling = NullValueHandling.Ignore)]
        public string Message { get; set; }

        [JsonProperty("name", NullValueHandling = NullValueHandling.Ignore)]
        public string Name { get; set; }
    }

    public partial class BaseError
    {
        public static BaseError FromJson(string json)
        {
            return JsonConvert.DeserializeObject<BaseError>(json, BitMEX.Model.BaseErrorConverter.Settings);
        }
    }

    internal static class BaseErrorConverter
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
