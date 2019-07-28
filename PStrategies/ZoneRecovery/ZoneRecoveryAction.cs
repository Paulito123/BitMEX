using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PStrategies.ZoneRecovery
{
    /// <summary>
    /// ZoneRecoveryAction class serves merely the purpose of transporting all the parameters needed for creating the orders in the
    /// application that uses this library.
    /// </summary>
    public class ZoneRecoveryAction
    {
        /// <summary>
        /// PositionIndex represents the number of positions taken in the past within the current strategy instance. It can be used 
        /// as a unique identifier by the application that uses this library to make sure the same action is not taken twice...
        /// </summary>
        public int PositionIndex { set; get; }

        /// <summary>
        /// The price where a profit is taken. TP = Take Profit
        /// </summary>
        public double TPPrice { set; get; }

        /// <summary>
        /// Volume to Sell all your Buy positions
        /// </summary>
        public double TPVolumeSell { set; get; }

        /// <summary>
        /// Volume to Buy all your Sell positions
        /// </summary>
        public double TPVolumeBuy { set; get; }

        /// <summary>
        /// The price at which the position should be reversed
        /// </summary>
        public double ReversePrice { set; get; }
        public double ReverseVolume { set; get; }

        /// <summary>
        /// Class constructor
        /// </summary>
        /// <param name="posIndex"></param>
        public ZoneRecoveryAction(int posIndex)
        {
            PositionIndex = posIndex;
        }
    }
}
