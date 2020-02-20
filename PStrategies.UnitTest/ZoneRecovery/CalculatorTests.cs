using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;


using PStrategies.ZoneRecovery;
using PStrategies.ZoneRecovery.State;

using Bitmex.Client.Websocket.Responses.Positions;
using Bitmex.Client.Websocket.Responses.Orders;

using BitMEXRest.Client;
using BitMEXRest.Dto;
using BitMEXRest.Model;

namespace PStrategies.UnitTest.ZoneRecovery
{
    [TestClass]
    public class CalculatorTests
    {
        [TestMethod]
        public void CalculatorTests_LifeCycleTest2Windings_ReturnsCorrectTPInProfit()
        {
            // Create a calculator instance
            var calcBox = new Calculator();

            Dictionary<ZoneRecoveryAccount, List<Order>> Orders = new Dictionary<ZoneRecoveryAccount, List<Order>>();
            Orders.Add(ZoneRecoveryAccount.A, new List<Order>());
            Orders.Add(ZoneRecoveryAccount.B, new List<Order>());
            Dictionary<ZoneRecoveryAccount, List<Position>> Positions = new Dictionary<ZoneRecoveryAccount, List<Position>>();
            Positions.Add(ZoneRecoveryAccount.A, new List<Position>());
            Positions.Add(ZoneRecoveryAccount.B, new List<Position>());
            Mutex PositionStatsMutex = new Mutex();

            // Create the Api Services
            var bitmexApiServiceA = BitmexApiService_Test_POS_Outcome.CreateDefaultApi("111");
            var bitmexApiServiceB = BitmexApiService_Test_POS_Outcome.CreateDefaultApi("222");

            // Initialize the Calculator
            calcBox.Initialize(
                    bitmexApiServiceA, bitmexApiServiceB,
                    1,
                    Orders,
                    Positions,
                    PositionStatsMutex,
                    "XBTUSD", 4, 50, (decimal)0.05, 1, (decimal)0.02);

            // Switch on the Calculator
            calcBox.SwitchedOn = true;

            // Update the market prices
            Dictionary<string, decimal> dict = new Dictionary<string, decimal>();
            dict.Add("Ask", 10000);
            dict.Add("Bid", 10000);
            
            if (dict != null)
                calcBox.UpdatePrices(dict);

            calcBox.Evaluate();

            var ordrA = calcBox.ZRBatchLedger[calcBox.RunningBatchNr].ZROrdersList.Where(x => x.Account == ZoneRecoveryAccount.A).Single();
            var ordrB = calcBox.ZRBatchLedger[calcBox.RunningBatchNr].ZROrdersList.Where(x => x.Account == ZoneRecoveryAccount.B).Single();

            Orders[ZoneRecoveryAccount.A].Add(new Order() {
                Account     = 111,
                Symbol      = "XBTUSD",
                OrdStatus   = OrderStatus.New,
                ClOrdId     = ordrA.PostParams.ClOrdID,
                Price       = (double)ordrA.PostParams.Price,
                OrderQty    = (long)ordrA.PostParams.OrderQty
            });

            calcBox.Evaluate(ZoneRecoveryAccount.A, new List<string>() { ordrA.PostParams.ClOrdID });

            Assert.AreEqual(typeof(ZRSWorking), calcBox.State.GetType());

            Orders[ZoneRecoveryAccount.B].Add(new Order() {
                Account     = 222,
                Symbol      = "XBTUSD",
                OrdStatus   = OrderStatus.New,
                ClOrdId     = ordrB.PostParams.ClOrdID,
                Price       = (double)ordrB.PostParams.Price,
                OrderQty    = (long)ordrB.PostParams.OrderQty
            });

            calcBox.Evaluate(ZoneRecoveryAccount.A, new List<string>() { ordrB.PostParams.ClOrdID });

            //Assert.AreEqual(typeof(ZRSWorking), calcBox.State.GetType());

            
            // Assert
            //Assert.AreEqual(1, nextStep.PositionIndex);
            //Assert.AreEqual(300, nextStep.ReversePrice);
            //Assert.AreEqual("123", nextStep.ReverseVolume);
            //Assert.AreEqual(1, nextStep.TPPrice);
            //Assert.AreEqual(1, nextStep.TPVolumeBuy);
            //Assert.AreEqual(1, nextStep.TPVolumeSell);

        }
    }
}