using System;
using System.Collections.Generic;
using BitMEX.Model;
using BitMEX.Utilities;

namespace BitMEX.Client
{
    /// <summary>
    /// API Response
    /// Object to encapsulate everything related to the received web response.
    /// </summary>
    public class ApiResponse
    {
        public Uri Uri { get; private set; }
        public int StatusCode { get; private set; }
        public IDictionary<string, string> Headers { get; private set; }
        public string Json { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse<T> /> class.
        /// </summary>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="headers">HTTP headers.</param>
        /// <param name="json">json (parsed HTTP body)</param>
        /// <param name="uri">URI of the response.</param>
        public ApiResponse(int statusCode, IDictionary<string, string> headers, string json, Uri uri = null)
        {
            this.Uri = uri;
            this.StatusCode = statusCode;
            this.Headers = headers;
            this.Json = json;
        }
        
        // Processes json responses
        public object ApiResponseProcessor()
        {
            object o;
            try
            {
                // Check the status code
                switch (this.StatusCode)
                {
                    case 200:
                        // json is some model type
                        o = this.ApiResponseDispatcher();
                        break;
                    case 400:
                    case 401:
                    case 403:
                    case 404:
                        // json is ougth to be error type
                        o = BaseError.FromJson(this.Json);
                        break;
                    default:
                        // build a BaseError
                        BaseError bErr = new BaseError();
                        bErr.Error.Name = "Unknown StatusCode";
                        bErr.Error.Message = "The status code returned is of an unknown type.";
                        o = bErr;
                        break;
                }

                return o;
            }
            catch (Exception exc)
            {
                o = new BaseError();
                ((BaseError)o).Error.Name = "ApiResponseError";
                ((BaseError)o).Error.Message = exc.ToString();
                return o;
            }
        }

        // Creates objects from a JSON response.
        private object ApiResponseDispatcher()
        {
            // Json is empty > Shit...
            if(this.Json == null || this.Json == "")
                return null;

            // Uri is empty > Shit...
            if (this.Uri == null || this.Uri.AbsolutePath == "")
                return null;

            object o;

            // Return the correct type based on the Uri
            switch (this.Uri.AbsolutePath)
            {
                case "/api/v1/order":
                    if(this.Json.Substring(0, 1) == "[")
                    {
                        o = OrdersResponse.FromJson(this.Json);
                    }
                    else
                    {
                        o = OrderResponse.FromJson(this.Json);
                    }
                    break;
                case "/api/v1/order/all":
                case "/api/v1/order/bulk":
                    o = OrdersResponse.FromJson(this.Json);
                    break;
                case "/api/v1/order/closePosition":
                    o = OrderResponse.FromJson(this.Json);
                    break;
                case "/api/v1/order/cancelAllAfter":
                default:
                    o = null;
                    break;
            }

            return o;
        }

        //private void ApiResponseDBHandler(object o)
        //{
        //    try
        //    {
        //        DatabaseUtil dbUtil = new DatabaseUtil("connstring", "provider");

        //        switch (o.GetType().ToString())
        //        {
        //            case "BitMEX.Model.OrderResponse":
        //                dbUtil.WriteResponseToDB();
        //                break;
        //            case "System.Collections.Generic.List<BitMEX.Model.OrderResponse>":
        //                break;
        //        }
        //    }
        //    catch (Exception exc)
        //    {

        //    }
        //}
    }
}
