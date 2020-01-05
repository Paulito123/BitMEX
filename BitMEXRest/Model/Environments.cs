using System.Collections.Generic;
using BitMEXRest.Client;

namespace BitMEXRest.Model
{
    public static class Environments
    {
        public static readonly IDictionary<BitmexEnvironment, string> Values = new Dictionary<BitmexEnvironment, string>
        {
            {BitmexEnvironment.Test, "testnet.bitmex.com"},
            {BitmexEnvironment.Prod, "www.bitmex.com"}
        };
    }
}