using System;
using System.Collections.Generic;
using BitMEX.Model;
using BitMEX.Utilities;
using log4net;

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
        private ILog log;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApiResponse<T> /> class.
        /// </summary>
        /// <param name="statusCode">HTTP status code.</param>
        /// <param name="headers">HTTP headers. Currently not used...</param>
        /// <param name="json">json (parsed HTTP body)</param>
        /// <param name="uri">URI of the response.</param>
        public ApiResponse(int statusCode, IDictionary<string, string> headers, string json, ILog i, Uri uri = null)
        {
            this.Uri = uri;
            this.StatusCode = statusCode;
            this.Headers = headers;
            this.Json = json;
            this.log = i;
        }
        
        /// <summary>
        /// Depending on the http status code received, determine what is the expected type of the json string.
        /// </summary>
        /// <returns></returns>
        public object ApiResponseProcessor()
        {
            object o;
            try
            {
                // Check the status code
                switch (this.StatusCode)
                {
                    case 200:
                        o = this.ApiResponseDispatcher();
                        break;
                    case 400:
                    case 401:
                    case 403:
                    case 404:
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
                log.Info("ApiResponse statuscode [" + StatusCode + "] for uri [" + Uri + "]");
                return o;
            }
            catch (Exception exc)
            {
                o = new BaseError();
                ((BaseError)o).Error.Name = "ApiResponseError";
                ((BaseError)o).Error.Message = exc.ToString();
                log.Error("ApiResponse error [" + exc.ToString() + "]");
                return o;
            }
        }

        /// <summary>
        /// Finds the expected response object derived from the URI of the call.
        /// </summary>
        /// <returns></returns>
        private object ApiResponseDispatcher()
        {
            // Json is empty > Shit...
            if(String.IsNullOrEmpty(Json))
            {
                log.Error("Json == null or empty");
                return null;
            }
                
            // Uri is empty > Shit...
            if (this.Uri == null || this.Uri.AbsolutePath == "")
            {
                log.Error("Uri.AbsolutePath null or empty");
                return null;
            }
                
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
                    log.Error("Uri unknown. Please add [" + this.Uri.AbsolutePath + "] to the evaluated Uri's.");
                    break;
            }

            return o;
        }
    }
}
