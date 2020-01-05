using Newtonsoft.Json;

namespace BitMEXRest.Model
{
    public abstract class QueryJsonParams : IJsonQueryParams
    {
        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
