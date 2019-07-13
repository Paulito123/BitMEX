/* 
 * BitMEX Testnet API
 *
 * ## REST API for the BitMEX Trading Platform  [View Changelog](/app/apiChangelog)  - -- -  #### Getting Started  Base URI: [https://testnet.bitmex.com/api/v1](/api/v1)  ##### Fetching Data  All REST endpoints are documented below. You can try out any query right from this interface.  Most table queries accept `count`, `start`, and `reverse` params. Set `reverse=true` to get rows newest-first.  Additional documentation regarding filters, timestamps, and authentication is available in [the main API documentation](/app/restAPI).  *All* table data is available via the [Websocket](/app/wsAPI). We highly recommend using the socket if you want to have the quickest possible data without being subject to ratelimits.  ##### Return Types  By default, all data is returned as JSON. Send `?_format=csv` to get CSV data or `?_format=xml` to get XML data.  ##### Trade Data Queries  *This is only a small subset of what is available, to get you started.*  Fill in the parameters and click the `Try it out!` button to try any of these queries.  * [Pricing Data](#!/Quote/Quote_get)  * [Trade Data](#!/Trade/Trade_get)  * [OrderBook Data](#!/OrderBook/OrderBook_getL2)  * [Settlement Data](#!/Settlement/Settlement_get)  * [Exchange Statistics](#!/Stats/Stats_history)  Every function of the BitMEX.com platform is exposed here and documented. Many more functions are available.  ##### Swagger Specification  [⇩ Download Swagger JSON](swagger.json)  - -- -  ## All API Endpoints  Click to expand a section. 
 *
 * OpenAPI spec version: 1.2.0
 * Contact: support@bitmex.com
 * Generated by: https://github.com/swagger-api/swagger-codegen.git
 */

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.ComponentModel.DataAnnotations;
using SwaggerDateConverter = IO.Swagger.Client.SwaggerDateConverter;

namespace IO.Swagger.Model
{
    /// <summary>
    /// TradeBin
    /// </summary>
    [DataContract]
    public partial class TradeBin :  IEquatable<TradeBin>, IValidatableObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TradeBin" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected TradeBin() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TradeBin" /> class.
        /// </summary>
        /// <param name="timestamp">timestamp (required).</param>
        /// <param name="symbol">symbol (required).</param>
        /// <param name="open">open.</param>
        /// <param name="high">high.</param>
        /// <param name="low">low.</param>
        /// <param name="close">close.</param>
        /// <param name="trades">trades.</param>
        /// <param name="volume">volume.</param>
        /// <param name="vwap">vwap.</param>
        /// <param name="lastSize">lastSize.</param>
        /// <param name="turnover">turnover.</param>
        /// <param name="homeNotional">homeNotional.</param>
        /// <param name="foreignNotional">foreignNotional.</param>
        public TradeBin(DateTime? timestamp = default(DateTime?), string symbol = default(string), double? open = default(double?), double? high = default(double?), double? low = default(double?), double? close = default(double?), decimal? trades = default(decimal?), decimal? volume = default(decimal?), double? vwap = default(double?), decimal? lastSize = default(decimal?), decimal? turnover = default(decimal?), double? homeNotional = default(double?), double? foreignNotional = default(double?))
        {
            // to ensure "timestamp" is required (not null)
            if (timestamp == null)
            {
                throw new InvalidDataException("timestamp is a required property for TradeBin and cannot be null");
            }
            else
            {
                this.Timestamp = timestamp;
            }
            // to ensure "symbol" is required (not null)
            if (symbol == null)
            {
                throw new InvalidDataException("symbol is a required property for TradeBin and cannot be null");
            }
            else
            {
                this.Symbol = symbol;
            }
            this.Open = open;
            this.High = high;
            this.Low = low;
            this.Close = close;
            this.Trades = trades;
            this.Volume = volume;
            this.Vwap = vwap;
            this.LastSize = lastSize;
            this.Turnover = turnover;
            this.HomeNotional = homeNotional;
            this.ForeignNotional = foreignNotional;
        }
        
        /// <summary>
        /// Gets or Sets Timestamp
        /// </summary>
        [DataMember(Name="timestamp", EmitDefaultValue=false)]
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// Gets or Sets Symbol
        /// </summary>
        [DataMember(Name="symbol", EmitDefaultValue=false)]
        public string Symbol { get; set; }

        /// <summary>
        /// Gets or Sets Open
        /// </summary>
        [DataMember(Name="open", EmitDefaultValue=false)]
        public double? Open { get; set; }

        /// <summary>
        /// Gets or Sets High
        /// </summary>
        [DataMember(Name="high", EmitDefaultValue=false)]
        public double? High { get; set; }

        /// <summary>
        /// Gets or Sets Low
        /// </summary>
        [DataMember(Name="low", EmitDefaultValue=false)]
        public double? Low { get; set; }

        /// <summary>
        /// Gets or Sets Close
        /// </summary>
        [DataMember(Name="close", EmitDefaultValue=false)]
        public double? Close { get; set; }

        /// <summary>
        /// Gets or Sets Trades
        /// </summary>
        [DataMember(Name="trades", EmitDefaultValue=false)]
        public decimal? Trades { get; set; }

        /// <summary>
        /// Gets or Sets Volume
        /// </summary>
        [DataMember(Name="volume", EmitDefaultValue=false)]
        public decimal? Volume { get; set; }

        /// <summary>
        /// Gets or Sets Vwap
        /// </summary>
        [DataMember(Name="vwap", EmitDefaultValue=false)]
        public double? Vwap { get; set; }

        /// <summary>
        /// Gets or Sets LastSize
        /// </summary>
        [DataMember(Name="lastSize", EmitDefaultValue=false)]
        public decimal? LastSize { get; set; }

        /// <summary>
        /// Gets or Sets Turnover
        /// </summary>
        [DataMember(Name="turnover", EmitDefaultValue=false)]
        public decimal? Turnover { get; set; }

        /// <summary>
        /// Gets or Sets HomeNotional
        /// </summary>
        [DataMember(Name="homeNotional", EmitDefaultValue=false)]
        public double? HomeNotional { get; set; }

        /// <summary>
        /// Gets or Sets ForeignNotional
        /// </summary>
        [DataMember(Name="foreignNotional", EmitDefaultValue=false)]
        public double? ForeignNotional { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class TradeBin {\n");
            sb.Append("  Timestamp: ").Append(Timestamp).Append("\n");
            sb.Append("  Symbol: ").Append(Symbol).Append("\n");
            sb.Append("  Open: ").Append(Open).Append("\n");
            sb.Append("  High: ").Append(High).Append("\n");
            sb.Append("  Low: ").Append(Low).Append("\n");
            sb.Append("  Close: ").Append(Close).Append("\n");
            sb.Append("  Trades: ").Append(Trades).Append("\n");
            sb.Append("  Volume: ").Append(Volume).Append("\n");
            sb.Append("  Vwap: ").Append(Vwap).Append("\n");
            sb.Append("  LastSize: ").Append(LastSize).Append("\n");
            sb.Append("  Turnover: ").Append(Turnover).Append("\n");
            sb.Append("  HomeNotional: ").Append(HomeNotional).Append("\n");
            sb.Append("  ForeignNotional: ").Append(ForeignNotional).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }
  
        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as TradeBin);
        }

        /// <summary>
        /// Returns true if TradeBin instances are equal
        /// </summary>
        /// <param name="input">Instance of TradeBin to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TradeBin input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Timestamp == input.Timestamp ||
                    (this.Timestamp != null &&
                    this.Timestamp.Equals(input.Timestamp))
                ) && 
                (
                    this.Symbol == input.Symbol ||
                    (this.Symbol != null &&
                    this.Symbol.Equals(input.Symbol))
                ) && 
                (
                    this.Open == input.Open ||
                    (this.Open != null &&
                    this.Open.Equals(input.Open))
                ) && 
                (
                    this.High == input.High ||
                    (this.High != null &&
                    this.High.Equals(input.High))
                ) && 
                (
                    this.Low == input.Low ||
                    (this.Low != null &&
                    this.Low.Equals(input.Low))
                ) && 
                (
                    this.Close == input.Close ||
                    (this.Close != null &&
                    this.Close.Equals(input.Close))
                ) && 
                (
                    this.Trades == input.Trades ||
                    (this.Trades != null &&
                    this.Trades.Equals(input.Trades))
                ) && 
                (
                    this.Volume == input.Volume ||
                    (this.Volume != null &&
                    this.Volume.Equals(input.Volume))
                ) && 
                (
                    this.Vwap == input.Vwap ||
                    (this.Vwap != null &&
                    this.Vwap.Equals(input.Vwap))
                ) && 
                (
                    this.LastSize == input.LastSize ||
                    (this.LastSize != null &&
                    this.LastSize.Equals(input.LastSize))
                ) && 
                (
                    this.Turnover == input.Turnover ||
                    (this.Turnover != null &&
                    this.Turnover.Equals(input.Turnover))
                ) && 
                (
                    this.HomeNotional == input.HomeNotional ||
                    (this.HomeNotional != null &&
                    this.HomeNotional.Equals(input.HomeNotional))
                ) && 
                (
                    this.ForeignNotional == input.ForeignNotional ||
                    (this.ForeignNotional != null &&
                    this.ForeignNotional.Equals(input.ForeignNotional))
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.Timestamp != null)
                    hashCode = hashCode * 59 + this.Timestamp.GetHashCode();
                if (this.Symbol != null)
                    hashCode = hashCode * 59 + this.Symbol.GetHashCode();
                if (this.Open != null)
                    hashCode = hashCode * 59 + this.Open.GetHashCode();
                if (this.High != null)
                    hashCode = hashCode * 59 + this.High.GetHashCode();
                if (this.Low != null)
                    hashCode = hashCode * 59 + this.Low.GetHashCode();
                if (this.Close != null)
                    hashCode = hashCode * 59 + this.Close.GetHashCode();
                if (this.Trades != null)
                    hashCode = hashCode * 59 + this.Trades.GetHashCode();
                if (this.Volume != null)
                    hashCode = hashCode * 59 + this.Volume.GetHashCode();
                if (this.Vwap != null)
                    hashCode = hashCode * 59 + this.Vwap.GetHashCode();
                if (this.LastSize != null)
                    hashCode = hashCode * 59 + this.LastSize.GetHashCode();
                if (this.Turnover != null)
                    hashCode = hashCode * 59 + this.Turnover.GetHashCode();
                if (this.HomeNotional != null)
                    hashCode = hashCode * 59 + this.HomeNotional.GetHashCode();
                if (this.ForeignNotional != null)
                    hashCode = hashCode * 59 + this.ForeignNotional.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// To validate all properties of the instance
        /// </summary>
        /// <param name="validationContext">Validation context</param>
        /// <returns>Validation Result</returns>
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

}
