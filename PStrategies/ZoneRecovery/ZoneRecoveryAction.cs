namespace PStrategies.ZoneRecovery
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// ZoneRecoveryAction class serves merely the purpose of transporting all the parameters needed for creating the orders in the
    /// application that uses this library. It contains all the instructions for the next move.
    /// </summary>
    public class ZoneRecoveryAction
    {
        public long AccountNumber { set; get; }
        public List<string> OrdersToClose { set; get; }
        public string OrderID { set; get; }
        public double Price { set; get; }
        public long Qty { set; get; }
        public string Instruction { set; get; } // TP | TL | REV
        
        /// <summary>
        /// Class constructor
        /// </summary>
        public ZoneRecoveryAction(long account, List<string> ordersToClose, string orderID, long qty, double price, string instruction)
        {
            AccountNumber = account;
            OrdersToClose = ordersToClose;
            OrderID = orderID;
            Qty = qty;
            Price = price;
            Instruction = instruction;
        }
    }
}
