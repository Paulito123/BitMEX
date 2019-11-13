//using ServiceStack.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
//using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using BitMEX.Model;
using System.Diagnostics;
using log4net;

namespace BitMEX.Client
{
    public class MordoR
    {
        // TODO IDEA Let all methods that query the Exchange return a fixed object type. 
        // When error is returned, write it to the log.

        #region Variables

        private string  Domain = "https://testnet.bitmex.com";
        private string  ApiKey;
        private string  ApiSecret;
        public long Account { get; }

        #endregion End variables

        #region Constructor

        // TESTLONG  [51091]    : "QbpGewiOyIYMbyQ-ieaTKfOJ" - "FqGOSAewtkMBIuiIQHI47dxc6vBm3zqARSEr4Qif8K8N5eHf"
        // TESTSHORT [170591]   : "xEuMT-y7ffwxrvHA2yDwL1bZ" - "3l0AmJz7l3P47-gK__LwgZQQ23uOKCFhYJG4HeTLlGXadRm6"

        public MordoR(string bitmexKey, 
                      string bitmexSecret, 
                      string bitmexDomain = "https://testnet.bitmex.com")
        {
            this.ApiKey = bitmexKey;
            this.ApiSecret = bitmexSecret;
            this.Domain = bitmexDomain;

            try
            {
                this.Account = GetExchangeAccountNumber();
            }
            catch (Exception ex)
            {
                this.Account = 0;
            }
            

            //if (l == null)
            //{
            //    log4net.Config.XmlConfigurator.Configure();
            //    log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
            //}
            //else
            //    log = l;

            //log.Info(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType + " instantiated.");
        }

        #endregion Constructor

        #region JSON - Helpers

        /// <summary>
        /// Build a JSON object, encapsulated by {}
        /// </summary>
        /// <param name="param">Dictionary of parameters</param>
        /// <returns>an encapsulated list of parameters</returns>
        private string BuildJSONParamList(Dictionary<string, string> param)
        {
            if (param == null)
                return "";

            var entries = new List<string>();
            foreach (var item in param)
                entries.Add(string.Format("\"{0}\":\"{1}\"", item.Key, item.Value));

            return "{" + string.Join(",", entries) + "}";
        }

        /// <summary>
        /// Transforms an array of elements into a JSON array. 
        /// Example: Builds the ["XBTUSD","ADAZ19"] part in {"symbol":["XBTUSD","ADAZ19"]} from an array with values XBTUSD,ADAZ19.
        /// </summary>
        /// <param name="strArray">An array of elements</param>
        /// <returns>A JSON array composed of the elements in strArray</returns>
        private string BuildJSONArray(string[] strArray)
        {
            if (strArray != null && strArray.GetUpperBound(0) <= 0)
                return "";

            var entries = new List<string>();
            foreach (var item in strArray)
                entries.Add(string.Format("\"0\"", item.ToString()));

            return "[" + string.Join(",", entries) + "]";
        }

        #endregion JSON - Helpers

        #region API Connector - Helpers

        /// <summary>
        /// 
        /// Example for filter "filter": {"timestamp.time":"12:00", "timestamp.ww":6}
        /// {"symbol":["XBTUSD","ADAZ19"]}
        /// </summary>
        /// <param name="param"></param>
        /// <param name="depth"></param>
        /// <returns></returns>
        private string BuildQueryData(Dictionary<string, string> param)
        {
            if (param == null)
                return "";

            StringBuilder b = new StringBuilder();
            foreach (var item in param)
                b.Append(string.Format("&{0}={1}", item.Key, WebUtility.UrlEncode(item.Value)));
            
            try { return b.ToString().Substring(1); }
            catch (Exception e)
            {
                //log.Error(e.Message);
                return e.Message;
            }
        }
        
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        /// <summary>
        /// Returns a unix number representaing a date in the future. 
        /// </summary>
        /// <returns>A UNIX date (long) in the future.</returns>
        private long GetExpires()
        {
            return ToUnixTimeSeconds(DateTimeOffset.UtcNow) + 3600; // 
        }

        private static long ToUnixTimeSeconds(DateTimeOffset dateTimeOffset)
        {
            var unixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var unixTimeStampInTicks = (dateTimeOffset.ToUniversalTime() - unixStart).Ticks;
            return unixTimeStampInTicks / TimeSpan.TicksPerSecond;
        }

        private byte[] hmacsha256(byte[] keyByte, byte[] messageBytes)
        {
            using (var hash = new HMACSHA256(keyByte))
            {
                return hash.ComputeHash(messageBytes);
            }
        }

        /// <summary>
        /// Query prepares and sends the Http call to the REST API of BitMEX.
        /// </summary>
        /// <param name="method">Http method used for the call: GET ¦ PUT ¦ POST ¦ DELETE</param>
        /// <param name="function">Uri part that specifies the function of the call.</param>
        /// <param name="refreshLimitInfo">Unused parameter for later implementation of rate limit info querying. (optional)</param>
        /// <param name="param">A dictionary of Key-Value pairs as parameters to be sent along with the call.</param>
        /// <param name="auth">Specify whether the call needs authentication or not. (optional)</param>
        /// <param name="json">Specifiy the format or the call (optional)</param>
        /// <param name="filter"></param>
        /// <returns>ApiResponse object that can be various types.</returns>
        private ApiResponse Query(string method, string function, Dictionary<string, string> param = null, bool auth = false, bool json = false, bool refreshLimitInfo = false)
        {
            string paramData = json ? BuildJSONParamList(param) : BuildQueryData(param);
            string url = "/api/v1" + function + ((method == "GET" && paramData != "") ? "?" + paramData : "");
            string postData = (method != "GET") ? paramData : "";

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Domain + url);
            webRequest.Method = method;

            if (auth)
            {
                string expires = GetExpires().ToString();
                string message = method + url + expires + postData;
                byte[] signatureBytes = hmacsha256(Encoding.UTF8.GetBytes(ApiSecret), Encoding.UTF8.GetBytes(message));
                string signatureString = ByteArrayToString(signatureBytes);

                webRequest.Headers.Add("api-expires", expires);
                webRequest.Headers.Add("api-key", ApiKey);
                webRequest.Headers.Add("api-signature", signatureString);
            }

            try
            {
                if (postData != "")
                {
                    webRequest.ContentType = json ? "application/json" : "application/x-www-form-urlencoded";
                    var data = Encoding.UTF8.GetBytes(postData);
                    using (var stream = webRequest.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                
                //using (WebResponse webResponse = webRequest.GetResponse())
                using (HttpWebResponse webResponse = (HttpWebResponse)webRequest.GetResponse())
                using (Stream str = webResponse.GetResponseStream())
                using (StreamReader sr = new StreamReader(str))
                {
                    Dictionary<string, string> respHeadr = new Dictionary<string, string>();
                    respHeadr.Add("content-type", webResponse.GetResponseHeader("content-type")) ;
                    respHeadr.Add("status", webResponse.GetResponseHeader("status"));

                    //return sr.ReadToEnd();
                    return new ApiResponse((int)webResponse.StatusCode, respHeadr, sr.ReadToEnd(), null, webResponse.ResponseUri);
                }
            }
            catch (WebException wex)
            {
                using (HttpWebResponse webResponse = (HttpWebResponse)wex.Response)
                {
                    if (webResponse == null)
                        return new ApiResponse(400, new Dictionary<string, string>(), "{ \"error\": { \"message\": \"WebResponse is NULL\", \"name\": \"NullWebResponse\" } }");

                    using (Stream str = webResponse.GetResponseStream())
                    using (StreamReader sr = new StreamReader(str))
                    {
                        Dictionary<string, string> respHeadr = new Dictionary<string, string>();
                        //respHeadr.Add("content-type", webResponse.GetResponseHeader("content-type"));
                        //respHeadr.Add("status", webResponse.GetResponseHeader("status"));

                        return new ApiResponse((int)webResponse.StatusCode, respHeadr, sr.ReadToEnd(), null, webResponse.ResponseUri);
                    }
                }
            }
            catch (Exception exc)
            {
                return new ApiResponse(400, new Dictionary<string, string>(), "{ \"error\": { \"message\": \"" + exc.ToString() + "\", \"name\": \"BadWebResponse\" } }");
            }
        }
        
        #endregion
        
        #region GET /order

        /// <summary>
        /// Return all orders that are resting in the ordrbook.4
        /// </summary>
        /// <param name="symbol">The symbol for which an order is placed e.g. XBTUSD</param>
        /// <returns></returns>
        public object GetOpenOrdersForSymbol(string symbol)
        {
            var param = new Dictionary<string, string>();
            param["symbol"] = symbol;
            param["reverse"] = true.ToString();
            param["filter"] = "{\"OrdStatus\": \"New\"}";
            ApiResponse res = Query("GET", "/order", param, true);

            // Deserialize JSON result
            return res.ApiResponseProcessor();
        }

        /// <summary>
        /// Get all the orders for a given ClOrdId.
        /// Filter example: "filter": {"timestamp.time":"12:00", "timestamp.ww":6}
        /// Code example: "{\"clOrdID\":\"" + ClOrdId + "\",\"ordStatus\":\"Filled\"}"
        /// TODO: Create a bulk GetOrdersForId's method.
        /// TODO: create a method to compose sub-hierarchies
        /// </summary>
        /// <param name="ClOrdId">The client side order ID used for the search.</param>
        /// <returns></returns>
        public object GetFilledOrderForId(string ClOrdId)
        {
            var param = new Dictionary<string, string>();
            param["reverse"] = true.ToString();
            param["clOrdID"] = ClOrdId;
            param["ordStatus"] = "Filled";
            ApiResponse res = Query("GET", "/order", param, true);

            // Deserialize JSON result
            return res.ApiResponseProcessor();
        }

        /// <summary>
        /// Get a list of OrderResponses out of a comma separated list of IDs.
        /// </summary>
        /// <param name="CSID">Comma separated list of ClOrdIds</param>
        /// <returns>List<OrderResponse></returns>
        public object GetOrdersForCSId(string CSID)
        {
            var param = new Dictionary<string, string>();

            if (CSID.Count(f => f == ',') == 0)
                param["clOrdID"] = CSID;
            else
                param["clOrdID"] = "[\"" + CSID.Replace(",","\",\"") + "\"]";
            
            param["reverse"] = true.ToString();
            ApiResponse res = Query("GET", "/order", param, true);

            // Deserialize JSON result
            return res.ApiResponseProcessor();
        }

        /// <summary>
        /// Get a list of OrderResponse objects for the ID's passed as arguments.
        /// </summary>
        /// <param name="symbol">The symbol</param>
        /// <param name="clOrdIDs">A list of order ids to fetch on the server</param>
        /// <returns>A list of OrderResponse objects</returns>
        public object GetOrderByID(string symbol, string[] clOrdIDs)
        {
            var param = new Dictionary<string, string>();
            param["symbol"] = symbol;
            param["clOrdID"] = (clOrdIDs.GetUpperBound(0) > 0) ? BuildJSONArray(clOrdIDs) : clOrdIDs[0];
            
            ApiResponse res = Query("GET", "/order", param, true);

            // Deserialize JSON result
            return res.ApiResponseProcessor();
        }

        #endregion

        #region PUT /order

        /// <summary>
        /// Send an orderID or origClOrdID to identify the order you wish to amend. Both order quantity and price can 
        /// be amended. Only one qty field can be used to amend. Use the leavesQty field to specify how much of the 
        /// order you wish to remain open. This can be useful if you want to adjust your position's delta by a certain 
        /// amount, regardless of how much of the order has already filled.
        /// > A leavesQty can be used to make a "Filled" order live again, if it is received within 60 seconds of the 
        /// fill. Like order placement, amending can be done in bulk. Simply send a request to PUT /api/v1/order/bulk 
        /// with a JSON body of the shape: {"orders": [{...}, {...}]}, each object containing the fields used in this 
        /// endpoint.
        /// </summary>
        /// <param name="origClOrdID">A uniqueidentifier that identifies each order, as given with the POST order.</param>
        /// <param name="price">Optional limit price for 'Limit', 'StopLimit', and 'LimitIfTouched' orders.</param>
        /// <param name="orderQty">Number of contracts</param>
        /// <param name="leavesQty"></param>
        /// <param name="stopPx">Stop price</param>
        /// <returns></returns>
        public object AmendOrder(string origClOrdID, double price = 0.0, double orderQty = 0.0, double leavesQty = 0.0, double stopPx = 0.0)
        {
            var param = new Dictionary<string, string>();
            param["origClOrdID"] = origClOrdID;

            // Amend only the price
            if(price > 0 && orderQty == 0 && leavesQty == 00)
            {
                param["price"] = price.ToString();
            } // Amend 
            else if(price == 0 && orderQty != 0)
            {
                param["orderQty"] = orderQty.ToString();
            }
            else if (price > 0 && orderQty != 0)
            {
                param["price"] = price.ToString();
                param["orderQty"] = orderQty.ToString();
            }
            else if (price == 0 && leavesQty != 0)
            {
                param["leavesQty"] = leavesQty.ToString();
            }
            else if (price > 0 && leavesQty != 0)
            {
                param["price"] = price.ToString();
                param["leavesQty"] = leavesQty.ToString();
            }

            //if(stopPx != 0.0)
            //{
            //    param["stopPx"] = stopPx.ToString();
            //}

            if (param.Count > 1)
            {
                ApiResponse res = Query("PUT", "/order", param, true);
                return res.ApiResponseProcessor();
            }
            else
                return null;
        }
        #endregion

        #region POST /order

        /// <summary>
        /// Enter the market at the best available price.
        /// </summary>
        /// <param name="symbol">The symbol for which an order is placed e.g. XBTUSD</param>
        /// <param name="clOrdID">Locally generated ID is passed as the unique reference to this specific order.</param>
        /// <param name="orderQty">Number of contracts to trade e.g. 10</param>
        /// <returns>Object containing the response of the order.</returns>
        public object MarketOrder(string symbol, string clOrdID, long orderQty)
        {
            var param = new Dictionary<string, string>();
            param["symbol"] = symbol;
            param["side"] = (orderQty >= 0) ? "Buy" : "Sell";
            param["orderQty"] = orderQty.ToString();
            param["clOrdID"] = clOrdID;
            param["ordType"] = "Market";
            ApiResponse res = Query("POST", "/order", param, true);

            // Deserialize JSON result
            return res.ApiResponseProcessor();
        }

        /// <summary>
        /// Like a Stop Market, but enters a Limit order instead of a Market order. Specify an orderQty, stopPx, and price.
        /// </summary>
        /// <param name="symbol">The symbol for which an order is placed e.g. XBTUSD</param>
        /// <param name="clOrdID">Locally generated ID is passed as the unique reference to this specific order.</param>
        /// <param name="orderQty">Number of contracts to trade e.g. 10</param>
        /// <param name="stopPx">The stop price</param>
        /// <param name="price">When specified, StopLimit order is placed.</param>
        /// <returns>Object containing the response of the order.</returns>
        public object StopLimitOrder(string symbol, string clOrdID, long orderQty, double price, double stopPx)
        {
            var param = new Dictionary<string, string>();
            param["symbol"] = symbol;
            param["side"] = (orderQty >= 0) ? "Buy" : "Sell";
            param["clOrdID"] = clOrdID;
            param["stopPx"] = stopPx.ToString();
            param["orderQty"] = orderQty.ToString();
            param["price"] = price.ToString();
            
            ApiResponse res = Query("POST", "/order", param, true);
            
            return res.ApiResponseProcessor();
        }

        /// <summary>
        /// Puts a Buy order lower than the current price or Sell order above the current price in the orderbook. It is
        /// also known as a resting order.
        /// </summary>
        /// <param name="symbol">The symbol for which an order is placed e.g. XBTUSD</param>
        /// <param name="orderQty">Number of contracts to trade e.g. 10</param>
        /// <param name="price">Optional limit price for 'Limit', 'StopLimit', and 'LimitIfTouched' orders.</param>
        /// <param name="clOrdID">Locally generated ID is passed as the unique reference to this specific order.</param>
        /// <param name="options"></param>
        /// <returns>Object containing the response of the order.</returns>
        public object LimitOrder(string symbol, string clOrdID, long orderQty, double price, string options = "")
        {
            var param = new Dictionary<string, string>();
            param["symbol"] = symbol;
            param["side"] = (orderQty >= 0) ? "Buy" : "Sell";
            param["orderQty"] = orderQty.ToString();
            param["price"] = price.ToString();

            if (options == "AllOrNone")
            {
                param["displayQty"] = "0";
            }

            param["clOrdID"] = clOrdID;
            param["ordType"] = "Limit";

            // Check validity of execInst
            //string[] execInstArray = execInst.Split(',');
            //param["execInst"] = ""; // ParticipateDoNotInitiate (=Post-only)  | ReduceOnly | Close

            param["text"] = "Some text";

            ApiResponse res = Query("POST", "/order", param, true);

            // Deserialize JSON result
            return res.ApiResponseProcessor();
        }

        /// <summary>
        /// Because LONG orders cannot rest above the current price in the order book and SHORT orders cannot rest under 
        /// the current price in the order book, StopMarketOrders are placed on the server to fulfill just that purpose.
        /// The order then does not rest in the order book, but is known on the server and will be triggered once the
        /// stopPx is reached. It will then act as a MarketOrder.
        /// </summary>
        /// <param name="symbol"></param>
        /// <param name="clOrdID"></param>
        /// <param name="orderQty"></param>
        /// <param name="stopPx"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public object StopMarketOrder(string symbol, string clOrdID, long orderQty, double stopPx, string text = "Standard StopMarket")
        {
            var param = new Dictionary<string, string>();
            param["symbol"] = symbol;
            param["side"] = (orderQty >= 0) ? "Buy" : "Sell";
            param["stopPx"] = stopPx.ToString();
            param["orderQty"] = orderQty.ToString();
            param["clOrdID"] = clOrdID;
            param["ordType"] = "Stop";
            // MarkPrice, LastPrice, IndexPrice
            param["execInst"] = "MarkPrice";
            param["text"] = text;

            ApiResponse res = Query("POST", "/order", param, true);

            // Deserialize JSON result
            return res.ApiResponseProcessor();
        }
        
        #endregion

        #region DELETE /order

        /// <summary>
        /// Cancels a resting order.
        /// </summary>
        /// <param name="clOrdIDs">Collection of client order IDs to close</param>
        /// <param name="message">Informational message to be saved with the cancelation</param>
        /// <returns></returns>
        public object CancelOrders(string[] clOrdIDs, string message = "Cancel order...")
        {
            var param = new Dictionary<string, string>();
            param["clOrdID"] = BuildJSONArray(clOrdIDs);
            param["text"] = message;
            ApiResponse res = Query("DELETE", "/order", param, true, true);

            var r = res.ApiResponseProcessor();

            if(r is List<OrderResponse>)
            {
                if (((List<OrderResponse>)r).Count == 1)
                    return ((List<OrderResponse>)r).First();
            }

            return r;
        }

        #endregion

        #region /position

        /// <summary>
        /// Returns the account for this MordoR instance.
        /// </summary>
        /// <returns>Account number</returns>
        private long GetExchangeAccountNumber()
        {
            var resp = GetPositionsForSymbols(new string[] { "XBTUSD" });

            if (resp is List<PositionResponse>)
            {
                List<PositionResponse> lijst = (List<PositionResponse>)resp;
                if (lijst.Count > 0)
                    return lijst[0].Account;
                else
                    return 1;
            } 
            else
                return 0;
        }

        /// <summary>
        /// Get the current positions on the exchange for one or more symbols
        /// </summary>
        /// <param name="symbol">An array of symbol for whic the position needs to be returned</param>
        /// <returns>A list of PositionResponse objects, List<PositionResponse></returns>
        public object GetPositionsForSymbols(string[] symbol)
        {
            var param = new Dictionary<string, string>();

            if (symbol.GetUpperBound(0) == -1)
                return null;
            else if (symbol.GetUpperBound(0) == 0)
                param["filter"] = "{\"symbol\":\"" + symbol[0] + "\"}";
            else
                param["filter"] = "{\"symbol\":" + BuildJSONArray(symbol) + "}";

            ApiResponse res = Query("GET", "/position", param, true);

            // Deserialize JSON result
            return res.ApiResponseProcessor();
        }

        /// <summary>
        /// Set the leverage for a symbols position.
        /// Value of leverage must be an unsigned long between 0.01 and 100. 
        /// Send 0 to enable cross margin.
        /// </summary>
        /// <param name="symbol">Symbol for which the leverage needs to be set.</param>
        /// <param name="leverage">The value of the leverage to be set.</param>
        /// <returns></returns>
        public object SetPositionLeverage(string symbol, double leverage)
        {
            var param = new Dictionary<string, string>();
            param["symbol"] = symbol;
            param["leverage"] = leverage.ToString();
            ApiResponse res = Query("PUT", "/position/leverage", param, true);
            return res.ApiResponseProcessor();
        }

        #endregion

        #region /user/wallet

        /// <summary>
        /// Get the latest wallet balance known on the server.
        /// </summary>
        /// <param name="currency"></param>
        /// <returns>A WalletResponse object containing the wallet balances.</returns>
        public object GetWalletInfo(string currency = null)
        {
            var param = new Dictionary<string, string>();
            param["currency"] = (currency == null) ? "XBt" : currency;
            ApiResponse res = Query("GET", "/user/wallet", param, true);
            return res.ApiResponseProcessor();
        }

        #endregion

        #region /funding

        #endregion

        #region Helpers

        /// <summary>
        /// Generate a unique GUID
        /// </summary>
        /// <returns>returns a unique hash string</returns>
        public static string GenerateGUID()
        {
            return Guid.NewGuid().ToString("N");
        }

        /// <summary>
        /// The idea is to test the logger from outside MordoR...
        /// </summary>
        /// <param name="message">Message to be logged.</param>
        //public void LogInfoMessage(string message)
        //{
        //    log.Info(message);
        //}

        #endregion

        //public string PostOrders()
        //{
        //    var param = new Dictionary<string, string>();
        //    param["symbol"] = "XBTUSD";
        //    param["orderQty"] = "-1000";
        //    param["stopPx"] = "8750";
        //    param["ordType"] = "Stop";
        //    return TestQuery("POST", "/order", param, true);
        //}

        //private string TestQuery(string method, string function, Dictionary<string, string> param = null, bool auth = false, bool json = false)
        //{
        //    string paramData = json ? BuildJSONParamList(param) : BuildQueryData(param);
        //    string url = "/api/v1" + function + ((method == "GET" && paramData != "") ? "?" + paramData : "");
        //    string postData = (method != "GET") ? paramData : "";

        //    HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(Domain + url);
        //    webRequest.Method = method;

        //    if (auth)
        //    {
        //        string expires = GetExpires().ToString();
        //        string message = method + url + expires + postData;
        //        byte[] signatureBytes = hmacsha256(Encoding.UTF8.GetBytes(ApiSecret), Encoding.UTF8.GetBytes(message));
        //        string signatureString = ByteArrayToString(signatureBytes);

        //        webRequest.Headers.Add("api-expires", expires);
        //        webRequest.Headers.Add("api-key", ApiKey);
        //        webRequest.Headers.Add("api-signature", signatureString);
        //    }

        //    try
        //    {
        //        if (postData != "")
        //        {
        //            webRequest.ContentType = json ? "application/json" : "application/x-www-form-urlencoded";
        //            var data = Encoding.UTF8.GetBytes(postData);
        //            using (var stream = webRequest.GetRequestStream())
        //            {
        //                stream.Write(data, 0, data.Length);
        //            }
        //        }

        //        using (WebResponse webResponse = webRequest.GetResponse())
        //        using (Stream str = webResponse.GetResponseStream())
        //        using (StreamReader sr = new StreamReader(str))
        //        {
        //            return sr.ReadToEnd();
        //        }
        //    }
        //    catch (WebException wex)
        //    {
        //        using (HttpWebResponse response = (HttpWebResponse)wex.Response)
        //        {
        //            if (response == null)
        //                throw;

        //            using (Stream str = response.GetResponseStream())
        //            {
        //                using (StreamReader sr = new StreamReader(str))
        //                {
        //                    return sr.ReadToEnd();
        //                }
        //            }
        //        }
        //    }
        //}

    }
}

        /* XXXX GENERAL INFO XXXX
         * Symbol:      The symbol for which an order is placed e.g. XBTUSD
         * < orderQty >   Number of contracts to trade e.g. 10
         * < price >      Optional limit price for 'Limit', 'StopLimit', and 'LimitIfTouched' orders.
         * < OrderType >  The order types > Market ¦ Limit ¦ Stop ¦ StopLimit ¦ MarketIfTouched ¦ LimitIfTouched ¦ Pegged
         * < execInst >
         * Optional execution instructions. Valid options: 
         * - ParticipateDoNotInitiate: Post-only orders. Puts a volume in the orderbook only if it is not immediately executed.
         *      Possible with Limit Order, Stop Limit Order or Take Profit Limit Order
         * - AllOrNone 
         * - MarkPrice 
         * - IndexPrice 
         * - LastPrice 
         * - Close: 
         * - ReduceOnly 
         * - Fixed. 
         * 'AllOrNone' instruction requires displayQty to be 0. 
         * 'MarkPrice', 'IndexPrice' or 'LastPrice' instruction 
         * valid for 'Stop', 'StopLimit', 'MarketIfTouched', and 'LimitIfTouched' 
         * orders.
         * 
         * Defaults to 'Limit' when price is specified. 
         * Defaults to 'Stop' when stopPx is specified. 
         * Defaults to 'StopLimit' when price and stopPx are specified.
         * 
         * Orders:
         *      Market
         *      Limit
         *      Stop
         *      StopLimit
         *      MarketIfTouched
         *      LimitIfTouched
         *      Pegged
         *      
         * XXXX stopPx XXXX
         * Optional trigger price for 'Stop', 'StopLimit', 'MarketIfTouched', and 'LimitIfTouched' orders. Use a price below 
         * the current price for stop-sell orders and buy-if-touched orders. Use execInst of 'MarkPrice' or 'LastPrice' to 
         * define the current price used for triggering.
         param["stopPx"] = "";
         */
