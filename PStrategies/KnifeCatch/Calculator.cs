using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PStrategies.KnifeCatch
{
    /*
     * The idea is to have a strategy that calculates the price at which an order should be resting. 
     * It should only be filled at a time when there is extreme volatility. It should also calculate 
     * the preferred exit price in advance.
     * Steps to take:
     *  1. Do analysis of the past and determine a percentage by which extreme drops or rises occur.
     *  2. Create this class that helps calculating entry and exit points.
     *  3. Testing testing testing...
     *  
     *  Dit is test commentaar -OG
     *  
     */
    public class Calculator
    {
        #region Constructor(s)
        /// <summary>
        /// Creates a new instance of the KnifeCatch.Calculator class
        /// </summary>
        /// <param name=""></param>
        public Calculator()
        {
            
        }
        #endregion Constructor(s)
    }
}
