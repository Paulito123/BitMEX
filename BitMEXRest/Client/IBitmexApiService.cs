using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitMEXRest.Model;

namespace BitMEXRest.Client
{
    public interface IBitmexApiService
    {
        Task<BitmexApiResult<TResult>> Execute<TParams, TResult>(ApiActionAttributes<TParams, TResult> apiAction, TParams @params);
    }
}
