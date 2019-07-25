using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BitMEX.Model;

namespace PStrategies.ZoneRecovery
{
    public class Calculator
    {
        private object _lock;
        //private bool IsInOperation { get; set; }
        private static double[] FactorArray = new double[] { 1, 2, 3, 6, 12, 24, 48, 96 };
        private enum ZoneRecoveryStatus { Winding, Unwinding }
        private ZoneRecoveryStatus CurrentStatus;
        private List<ZoneRecoveryPosition> OpenPositions;

        /// <summary>
        /// CurrentDepth reflects the number of windings made.
        /// When 0 > Strategy has been initialized or is completely unwound. There should be no open positions.
        /// When 1 > First Winding / last Unwinding
        /// When CurrentDepth = MaxDepthIndex > ZoneRecoveryStatus should be switched and winding process reversed
        /// </summary>
        private int? CurrentDepth;

        public double? Price { get; set; }
        public double? MaxExposure { get; set; }
        public double? TotalBalance { get; set; }
        public double? Leverage { get; set; }
        public double? PipSize { get; set; }
        public int? MaxDepthIndex { get; set; }

        /// <summary>
        /// Initializes the Zone Recovery Calculator.
        /// </summary>
        /// <param name="price">price (required).</param>
        public Calculator(double price, double maxExposure, double totalBalance, double leverage, double pipSize, int maxDepthIndex)
        {
            CurrentStatus = ZoneRecoveryStatus.Winding;
            OpenPositions = new List<ZoneRecoveryPosition>();
            this.Price = price;
            this.MaxExposure = maxExposure;
            this.TotalBalance = totalBalance;
            this.Leverage = leverage;
            this.PipSize = pipSize;
            this.CurrentDepth = 0;
            this.MaxDepthIndex = maxDepthIndex;
        }

        ///// <summary>
        ///// Change the internal parameters of the Calculator. 
        ///// </summary>
        //public void ChangeParameters(double? price, double? maxExposure = null, double? totalBalance = null, double? leverage = null,
        //                             double? pipSize = null, int? maxDepthIndex = null)
        //{
        //    lock (_lock)
        //    {
        //        if (price != null)
        //            this.Price = price;

        //        if (maxExposure != null)
        //            this.MaxExposure = (double)maxExposure;

        //        if (totalBalance != null)
        //            this.TotalBalance = (double)totalBalance;

        //        if (leverage != null)
        //            this.Leverage = (double)leverage;

        //        if (pipSize != null)
        //            this.PipSize = (double)pipSize;

        //        if (maxDepthIndex != null && CurrentDepth != null && CurrentDepth <= (int)MaxDepthIndex)
        //            this.MaxDepthIndex = (int)maxDepthIndex;
        //    }
        //}

        /// <summary>
        /// Calculates the total maximum exposure (depth) possible, according to the depths defines in FactorArray. Basically
        /// this is the sum of all the factors defined in FactorArray until the defined MaxDepth. It represents the 
        /// maximum exposure possible.
        /// Example: 
        ///     FactorArray = new double[] { 1, 2, 3, 6, 12, 24, 48, 96 };
        ///     MaxDepth = 4
        ///     GetTotalDepthMaxExposure() returns 12 (1 + 2 + 3 + 6) 
        /// </summary>
        private double? GetTotalDepthMaxExposure()
        {
            if (this.MaxDepthIndex != null && this.MaxDepthIndex < FactorArray.GetUpperBound(0))
            {
                double sum = 0;

                for (int i = 0; i < this.MaxDepthIndex; i++)
                {
                    sum = sum + FactorArray[i];
                }
                return sum;
            }
            else
                return null;
        }

        /// <summary>
        /// For a given Price, Balance, MaxExposure and Leverage, this function returns the calculated maximum unit size. 
        /// This unit size multiplied with the factors in FactorArray will give the unit size of each order.
        /// </summary>
        public double? GetRelativeUnitSize()
        {
            if (this.MaxDepthIndex != null && this.MaxDepthIndex < FactorArray.GetUpperBound(0) && this.Price != null && this.TotalBalance != null && this.Leverage != null)
            {
                double totalDepthMaxExposure = (double)this.GetTotalDepthMaxExposure();
                return Math.Round((double)(Price * Leverage * TotalBalance * MaxExposure / totalDepthMaxExposure) * (1 / (double)PipSize), MidpointRounding.AwayFromZero) / (1 / (double)PipSize);
            }
            else
                return null;
        }

        /// <summary>
        /// SetNewPosition should be called when a new position is taken on the exchange. The Zone Recovery
        /// strategy proceeds one step in its logic.
        /// </summary>
        /// <param name="orderResp">The OrderResponse object returned by the Exchange</param>
        //public void SetNewPosition(OrderResponse orderResp)
        //{

        //}

        /// <summary>
        /// SetNewPosition should be called when a new position is taken on the exchange. The Zone Recovery
        /// strategy proceeds one step in its logic. The List of OrderResponses passed should be the 
        /// OrderResponses returned for one specific order.
        /// 
        /// TODO: Check how WebSocket returns updates on resting orders. This function makes the assumption 
        /// that a list of OrderResponses is returned.
        /// </summary>
        /// <param name="orderResp">The List object with all the OrderResponses for one order returned by the Exchange.</param>
        public void SetNewPosition(List<BitMEX.Model.OrderResponse> orderResp)
        {
            // Create a new position object for the calculation of the average position size
            ZoneRecoveryPosition newPos = new ZoneRecoveryPosition((double)this.PipSize, OpenPositions.Count + 1);

            // Loop all the OrderResponse objects related to a specific filled previously resting or market order.
            foreach (BitMEX.Model.OrderResponse resp in orderResp)
            {
                // TODO: Check if assumption is correct that AvgPx is the average price for the filled OrderQty...
                newPos.AddToPosition((double)resp.AvgPx, (double)resp.OrderQty);
            }

            // Add the new averaged position to the positions collection
            this.OpenPositions.Add(newPos);

            if(CurrentStatus == ZoneRecoveryStatus.Winding)
            {
                CurrentDepth++;

                // Zone Recovery logic is reversed
                if (CurrentDepth == MaxDepthIndex)
                    CurrentStatus = ZoneRecoveryStatus.Unwinding;
            }
            else // Unwinding...
                CurrentDepth--;
        }

        public ZoneRecoveryAction GetNextStep()
        {
            if(CurrentStatus == ZoneRecoveryStatus.Winding)
            {
                if (CurrentDepth == MaxDepthIndex)
                    CurrentStatus = ZoneRecoveryStatus.Unwinding;


            }
            else if(CurrentStatus == ZoneRecoveryStatus.Unwinding)
            {

            }
            return null;
        }
    }

    public class ZoneRecoveryAction
    {
        public double TPPrice;
        public double TPVolume;
        public double ReversePrice;
        public double ReverseVolume;
        public double Price;
        public double Volume;

        public ZoneRecoveryAction(double price, double volume)
        {
            this.Price = price;
            this.Volume = volume;
        }
    }

    /// <summary>
    /// Class that represents a taken position. Since one order could be filled by a number of OrderResponses, it is
    /// easier to have a calculated object with only the information needed rather than the collection of
    /// OrderResponses.
    /// When Volume is negative, the position is a Sell position.
    /// When Volume is position, the position is a Long position.
    /// </summary>
    public class ZoneRecoveryPosition
    {
        public double AVGPrice { get; set; }
        public double TotalVolume { get; set; }
        public double PipSize { get; set; }
        public int ZRIndex { get; set; }

        public void AddToPosition(double executionPrice, double executionVolume)
        {
            this.AVGPrice = CalculateAveragePrice(AVGPrice, executionPrice, TotalVolume, executionVolume, PipSize);
            this.TotalVolume = this.TotalVolume + executionVolume;
        }

        public ZoneRecoveryPosition(double pipSize, int zrIndex)
        {
            this.AVGPrice = 0.0;
            this.TotalVolume = 0.0;
            this.PipSize = pipSize;
            this.ZRIndex = ZRIndex;
        }

        public static double CalculateAveragePrice(double price1, double price2, double vol1, double vol2, double pipSize)
        {
            return Math.Round(((price1 * vol1) + (price2 * vol2)) / (vol1 + vol2) * (1 / pipSize), MidpointRounding.AwayFromZero) / (1 / pipSize);
        }
    }
}
