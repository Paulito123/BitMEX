using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using BitMEXRest.Model;
using Serilog;

namespace BitMEXRest.Client
{
    public class BitmexApiService : IBitmexApiService
    {
        private readonly IBitmexApiProxy _bitmexApiProxy;

        public BitmexApiService(IBitmexApiProxy bitmexApiProxy)
        {
            _bitmexApiProxy = bitmexApiProxy;
        }

        protected BitmexApiService(IBitmexAuthorization bitmexAuthorization)
        {
            _bitmexApiProxy = new BitmexApiProxy(bitmexAuthorization);
        }

        public async Task<BitmexApiResult<TResult>> Execute<TParams, TResult>(ApiActionAttributes<TParams, TResult> apiAction, TParams @params)
        {
            switch (apiAction.Method)
            {
                case HttpMethods.GET:
                    {
                        var getQueryParams = @params as IQueryStringParams;
                        //Log.Debug(getQueryParams.ToQueryString());
                        var serializedResult = await _bitmexApiProxy.Get(apiAction.Action, getQueryParams);
                        //Log.Debug(serializedResult.Result);
                        var deserializedResult = JsonConvert.DeserializeObject<TResult>(serializedResult.Result);
                        return serializedResult.ToResultType<TResult>(deserializedResult);
                    }
                case HttpMethods.POST:
                    {
                        var postQueryParams = @params as IJsonQueryParams;
                        //Log.Debug("1." + postQueryParams.ToJson());
                        var serializedResult = await _bitmexApiProxy.Post(apiAction.Action, postQueryParams);
                        //Log.Debug("2." + serializedResult.Result);
                        var deserializedResult = JsonConvert.DeserializeObject<TResult>(serializedResult.Result);
                        return serializedResult.ToResultType<TResult>(deserializedResult);
                    }
                case HttpMethods.PUT:
                    {
                        var putQueryParams = @params as IJsonQueryParams;
                        //Log.Debug(putQueryParams.ToJson());
                        var serializedResult = await _bitmexApiProxy.Put(apiAction.Action, putQueryParams);
                        //Log.Debug(serializedResult.Result);
                        var deserializedResult = JsonConvert.DeserializeObject<TResult>(serializedResult.Result);
                        return serializedResult.ToResultType<TResult>(deserializedResult);
                    }
                case HttpMethods.DELETE:
                    {
                        var deleteQueryParams = @params as IQueryStringParams;
                        //Log.Debug(deleteQueryParams.ToQueryString());
                        var serializedResult = await _bitmexApiProxy.Delete(apiAction.Action, deleteQueryParams);
                        //Log.Debug(serializedResult.Result);
                        var deserializedResult = JsonConvert.DeserializeObject<TResult>(serializedResult.Result);
                        return serializedResult.ToResultType<TResult>(deserializedResult);
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static IBitmexApiService CreateDefaultApi(IBitmexAuthorization bitmexAuthorization)
        {
            return new BitmexApiService(bitmexAuthorization);
        }
    }

    public class BitmexApiService_Test_POS_Outcome : IBitmexApiService
    {
        public async Task<BitmexApiResult<TResult>> Execute<TParams, TResult>(ApiActionAttributes<TParams, TResult> apiAction, TParams @params)
        {
            switch (apiAction.Method)
            {
                case HttpMethods.POST:
                    {
                        var postQueryParams = @params as IJsonQueryParams;
                        dynamic data = JsonConvert.DeserializeObject(postQueryParams.ToJson());
                        string DummyId = DateTime.Now.ToLongDateString();
                        var serializedResult = new BitmexApiResult<string>($"{{\"orderID\":\"{DummyId}\",\"clOrdID\":\"{data.clOrdID}\",\"account\":11111,\"symbol\":\"{data.symbol}\",\"side\":\"{data.side}\",\"orderQty\":{data.orderQty},\"price\":{data.price},\"stopPx\":null,\"ordType\":\"{data.ordType}\",\"timeInForce\":\"GoodTillCancel\",\"execInst\":\"ParticipateDoNotInitiate\",\"ordStatus\":\"New\",\"triggered\":\"\",\"workingIndicator\":true,\"transactTime\":\"{string.Format("{0:s}", DateTime.Now)}.000Z\",\"timestamp\":\"{string.Format("{0:s}", DateTime.Now)}.000Z\"}}", 60, 59, DateTime.Now);
                        var deserializedResult = JsonConvert.DeserializeObject<TResult>(serializedResult.Result);
                        return serializedResult.ToResultType<TResult>(deserializedResult);
                    }
                case HttpMethods.DELETE:
                    {
                        var deleteQueryParams = (@params as IQueryStringParams).ToQueryString();
                        var clOrdId = deleteQueryParams.Substring(deleteQueryParams.IndexOf(@"&") + 9, deleteQueryParams.LastIndexOf(@"&") - (deleteQueryParams.IndexOf(@"&") + 9));
                        string DummyId = DateTime.Now.ToLongDateString();
                        var serializedResult = new BitmexApiResult<string>($"[{{\"orderID\":\"{DummyId}\",\"clOrdID\":\"{clOrdId}\",\"account\":11111,\"symbol\":\"XBTUSD\",\"side\":\"Sell\",\"orderQty\":1,\"price\":1,\"stopPx\":null,\"ordType\":\"Limit\",\"timeInForce\":\"GoodTillCancel\",\"execInst\":\"ParticipateDoNotInitiate\",\"ordStatus\":\"Canceled\",\"triggered\":\"\",\"workingIndicator\":true,\"transactTime\":\"{string.Format("{0:s}", DateTime.Now)}.000Z\",\"timestamp\":\"{string.Format("{0:s}", DateTime.Now)}.000Z\"}}]", 60, 59, DateTime.Now);
                        var deserializedResult = JsonConvert.DeserializeObject<TResult>(serializedResult.Result);
                        return serializedResult.ToResultType<TResult>(deserializedResult);
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static IBitmexApiService CreateDefaultApi()
        {
            return new BitmexApiService_Test_POS_Outcome();
        }
    }
}
