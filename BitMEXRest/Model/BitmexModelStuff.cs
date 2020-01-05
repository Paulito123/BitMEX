namespace BitMEXRest.Model
{
    public enum OrderType
    {
        Market = 1,
        Limit = 2,
        Stop = 3,
    }

    public enum OrderSide
    {
        Buy = 1,
        Sell = 2
    }

    public enum HttpMethods
    {
        GET = 1,
        POST = 2,
        PUT = 3,
        DELETE = 4
    }
}
