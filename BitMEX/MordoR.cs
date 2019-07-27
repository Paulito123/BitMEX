//using ServiceStack.Text;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
//using System.Globalization;
using System.IO;
//using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using BitMEX.Model;
using BitMEX.Client;
using System.Diagnostics;

namespace BitMEX
{
    public class MordoR
    {
        private string domain = "https://testnet.bitmex.com";
        private string apiKey;
        private string apiSecret;

        public MordoR(string bitmexKey = "rTAFXRKn2dLARuG_t1dDOtgI", string bitmexSecret = "K2LmL6aTbj8eW_LVj7OLa7iA6eZa8TJMllh3sjCynV4fpnMr", string bitmexDomain = "https://testnet.bitmex.com")
        {
            this.apiKey = bitmexKey;
            this.apiSecret = bitmexSecret;
            this.domain = bitmexDomain; 
        }

        #region API Connector - Helpers
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
                //StaticLogger.LogError(this.GetType(), e.Message);
                return e.Message;
            }
        }

        private string BuildJSON(Dictionary<string, string> param)
        {
            if (param == null)
                return "";

            var entries = new List<string>();
            foreach (var item in param)
                entries.Add(string.Format("\"{0}\":\"{1}\"", item.Key, item.Value));

            return "{" + string.Join(",", entries) + "}";
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private long GetExpires()
        {
            return ToUnixTimeSeconds(DateTimeOffset.UtcNow) + 3600; // set expires one hour in the future
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

        /*
         * method: HTTP method > GET ¦ PUT ¦ POST ¦ DELETE
         * function: HTTP suffix e.g. /order, /order/all, /position, etc...
         * refreshLimitInfo (optional): Unused parameter for later implementation of rate limit info querying...
         * param (optional): Dictionary of key - value pairs that contain parameters to be passed with the request
         * auth (optional)
         * json (optional)
         */
        private ApiResponse Query(string method, string function, bool refreshLimitInfo = false, Dictionary<string, string> param = null, bool auth = false, bool json = false)
        {
            string paramData = json ? BuildJSON(param) : BuildQueryData(param);
            string url = "/api/v1" + function + ((method == "GET" && paramData != "") ? "?" + paramData : "");
            string postData = (method != "GET") ? paramData : "";
            //ApiResponse outputResp;

            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(domain + url);
            webRequest.Method = method;

            if (auth)
            {
                string expires = GetExpires().ToString();
                string message = method + url + expires + postData;
                byte[] signatureBytes = hmacsha256(Encoding.UTF8.GetBytes(apiSecret), Encoding.UTF8.GetBytes(message));
                string signatureString = ByteArrayToString(signatureBytes);

                webRequest.Headers.Add("api-expires", expires);
                webRequest.Headers.Add("api-key", apiKey);
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
                    return new ApiResponse((int)webResponse.StatusCode, respHeadr, sr.ReadToEnd(), webResponse.ResponseUri);
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

                        return new ApiResponse((int)webResponse.StatusCode, respHeadr, sr.ReadToEnd(), webResponse.ResponseUri);
                    }
                }
            }
            catch (Exception exc)
            {
                return new ApiResponse(400, new Dictionary<string, string>(), "{ \"error\": { \"message\": \"" + exc.ToString() + "\", \"name\": \"BadWebResponse\" } }");
            }
        }
        #endregion

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

        #region GET /order
        /* XXXX GetOrders XXXX
         * Symbol:      The symbol for which an order is placed e.g. XBTUSD
         */
        public object GetOpenOrdersForSymbol(string symbol)
        {
            var param = new Dictionary<string, string>();
            param["symbol"] = symbol;
            ApiResponse res = Query("GET", "/order", false, param, true);

            // Deserialize JSON result
            return res.ApiResponseProcessor();
        }
        #endregion

        #region PUT /order
        /* XXXX AmendOrder XXXX
         * origClOrdID: A uniqueidentifier that identifies each order, as given with the POST order.
         * orderQty:    Number of contracts
         * price:       Optional limit price for 'Limit', 'StopLimit', and 'LimitIfTouched' orders.
         * 
         * Send an orderID or origClOrdID to identify the order you wish to amend. 
         * Both order quantity and price can be amended. Only one qty field can be used to amend.
         * Use the leavesQty field to specify how much of the order you wish to remain open. This 
         * can be useful if you want to adjust your position's delta by a certain amount, 
         * regardless of how much of the order has already filled.
         * > A leavesQty can be used to make a "Filled" order live again, if it is received within 
         * 60 seconds of the fill. 
         * Like order placement, amending can be done in bulk. Simply send a request to PUT 
         * /api/v1/order/bulk with a JSON body of the shape: {"orders": [{...}, {...}]}, each 
         * object containing the fields used in this endpoint.
         */
        public object AmendOrder(string origClOrdID, int orderQty, double price, double stopPx = 0.0)
        {
            var param = new Dictionary<string, string>();
            param["origClOrdID"] = origClOrdID;

            param["orderQty"] = orderQty.ToString();
            //param["leavesQty"] = "";
            param["price"] = price.ToString();

            if(stopPx != 0.0)
            {
                param["stopPx"] = stopPx.ToString();
            }
            
            ApiResponse res = Query("PUT", "/order", false, param, true);
            // Deserialize JSON result
            return res.ApiResponseProcessor();
        }
        #endregion

        #region POST /order
        /* XXXX MarketOrder XXXX
         * symbol:      The symbol for which an order is placed e.g. XBTUSD
         * orderQty:    Number of contracts to trade e.g. 10
         * clOrdID:     Locally generated ID is passed as the unique reference to this specific order.
         */
        public object MarketOrder(string symbol, out string clOrdID, int orderQty)
        {
            clOrdID = this.generateGUID();

            var param = new Dictionary<string, string>();
            param["symbol"] = symbol;
            param["side"] = (orderQty >= 0) ? "Buy" : "Sell";
            param["orderQty"] = orderQty.ToString();
            param["clOrdID"] = clOrdID;
            param["ordType"] = "Market";
            ApiResponse res = Query("POST", "/order", false, param, true);

            // Deserialize JSON result
            return res.ApiResponseProcessor();
        }

        /* XXXX StopOrder XXXX
         * symbol:      The symbol for which an order is placed e.g. XBTUSD
         * clOrdID:     Locally generated ID is passed as the unique reference to this specific order.
         * orderQty:    Number of contracts to trade e.g. 10
         */
        public object StopOrder(string symbol, int orderQty, double stopPx, out string clOrdID)
        {
            clOrdID = this.generateGUID();

            var param = new Dictionary<string, string>();
            param["symbol"] = symbol;
            param["side"] = (orderQty >= 0) ? "Buy" : "Sell";
            param["orderQty"] = orderQty.ToString();
            param["clOrdID"] = clOrdID;
            param["stopPx"] = stopPx.ToString();
            param["ordType"] = "Stop";
            param["execInst"] = "MarkPrice"; // Close, [ MarkPrice OR LastPrice OR IndexPrice ]
            ApiResponse res = Query("POST", "/order", false, param, true);

            // Deserialize JSON result
            return res.ApiResponseProcessor();
        }

        /* XXXX LimitOrder XXXX
         * Symbol:      The symbol for which an order is placed e.g. XBTUSD
         * < orderQty >   Number of contracts to trade e.g. 10
         * < price >      Optional limit price for 'Limit', 'StopLimit', and 'LimitIfTouched' orders.

         * < execInst >
         * Optional execution instructions. Valid options: 
         * - ParticipateDoNotInitiate: Post-only orders. Puts a volume in the orderbook only if it is not immediately executed.
         *      Possible with Limit Order, Stop Limit Order or Take Profit Limit Order
         * - AllOrNone: Either the whole order is filled or not at all.
         * - MarkPrice: 
         *      Possible with Stop, StopLimit, MarketIfTouched, and LimitIfTouched
         * - IndexPrice 
         * - LastPrice 
         * - Close: 
         * - ReduceOnly 
         * - Fixed. 
         * 'AllOrNone' instruction requires displayQty to be 0. 'MarkPrice', 'IndexPrice' or 'LastPrice' instruction 
         * valid for 'Stop', 'StopLimit', 'MarketIfTouched', and 'LimitIfTouched' 
         * orders.
         * 
         */
        public object LimitOrder(string symbol, int orderQty, double price, out string clOrdID, string options = "")
        {
            clOrdID = this.generateGUID();
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

            ApiResponse res = Query("POST", "/order", false, param, true);

            // Deserialize JSON result
            return res.ApiResponseProcessor();
        }
        #endregion

        #region DELETE /order
        /* XXXX CancelOrder XXXX
         * clOrdID: OrderID of the order that needs to be canceled
         * message: Informational message to be saved with the cancelation
         */
        public object CancelOrder(string clOrdID, string message = "Cancel order...")
        {
            var param = new Dictionary<string, string>();
            param["clOrdID"] = clOrdID;
            param["text"] = message;
            ApiResponse res = Query("DELETE", "/order", false, param, true, true);

            // Deserialize JSON result
            return res.ApiResponseProcessor();
        }
        #endregion

        #region /user/wallet

        #endregion
         
        #region /funding

        #endregion

        #region Helpers
        private string generateGUID()
        {
            return Guid.NewGuid().ToString("N");
        }

        //private object ProcessJSONOrderResponse(string res, Boolean isArray = false)
        //{
        //    if (isArray)
        //    {
        //        List<OrderResponse> multiOrderResp = OrdersResponse.FromJson(res);

        //        if (multiOrderResp.Count > 0)
        //        {
        //            return multiOrderResp;
        //        }
        //        else
        //        {
        //            var orderError = BaseError.FromJson(res);

        //            if (orderError.Error != null)
        //            {
        //                return orderError;
        //            }
        //            else
        //            {
        //                orderError = new BaseError();
        //                orderError.Error.Name = "Custom error";
        //                orderError.Error.Message = "No JSON response received...";
        //                return orderError;
        //            }
        //        }
        //    }
        //    else
        //    {
        //        var oderResp = OrderResponse.FromJson(res);

        //        if (oderResp.OrderId != null)
        //        {
        //            return oderResp;
        //        }
        //        else
        //        {
        //            var orderError = BaseError.FromJson(res);

        //            if (orderError.Error != null)
        //            {
        //                return orderError;
        //            }
        //            else
        //            {
        //                orderError = new BaseError();
        //                orderError.Error.Name = "Custom error";
        //                orderError.Error.Message = "No JSON response received...";
        //                return orderError;
        //            }
        //        }
        //    }
        //}
        #endregion
    }
}