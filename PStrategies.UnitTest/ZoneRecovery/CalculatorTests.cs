using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PStrategies.ZoneRecovery;
using System.Collections.Generic;
using BitMEX.Model;

namespace PStrategies.UnitTest.ZoneRecovery
{
    [TestClass]
    public class CalculatorTests
    {
        [TestMethod]
        public void CalculatorTests_LifeCycleTest2Windings_ReturnsCorrectTPInProfit()
        {
            var calcBox = new Calculator(1000, 0.05, 1, 10, 0.5, 4, 100, 0.01);

            #region Set new position 1
            var orderResponseList = new List<BitMEX.Model.OrderResponse>();
            
            var orderResponse = new BitMEX.Model.OrderResponse()
            {
                OrderId = "a758f23f-f767-739c-891f-3dff5e6d4558",
                ClOrdId = "a758f23f-f767-739c-891f-3dff5e6d4558",
                OrderQty = 1000,
                Price = 9103.5,
                LeavesQty = 0,
                CumQty = 1000,
                AvgPx = 9103.5,

                OrdType = "Limit",
                OrdStatus = "Filled",
                ExecInst = "",
                TimeInForce = "ImmediateOrCancel",

                Account = 51091,
                Symbol = "XBTUSD",
                Side = "Buy",
                StopPx = null,
                PegOffsetValue = null,
                PegPriceType = "",
                Currency = "USD",
                SettlCurrency = "XBt",
                ContingencyType = "",
                ExDestination = "XBME",
                Triggered = "",
                TransactTime = new DateTime(2019, 7, 7, 1, 1, 1, 1),
                Timestamp = new DateTime(2019, 7, 7, 1, 1, 1, 1)
            };
            orderResponseList.Add(orderResponse);

            orderResponse = new BitMEX.Model.OrderResponse()
            {
                OrderId = "a758f23f-f767-739c-891f-3dff5e6d4558",
                ClOrdId = "a758f23f-f767-739c-891f-3dff5e6d4558",
                OrderQty = 1000,
                Price = 1000,
                LeavesQty = 0,
                CumQty = 1000,
                AvgPx = 9103.5,

                OrdType = "Limit",
                OrdStatus = "Filled",
                ExecInst = "",
                TimeInForce = "ImmediateOrCancel",

                Account = 51091,
                Symbol = "XBTUSD",
                Side = "Buy",
                StopPx = null,
                PegOffsetValue = null,
                PegPriceType = "",
                Currency = "USD",
                SettlCurrency = "XBt",
                ContingencyType = "",
                ExDestination = "XBME",
                Triggered = "",
                TransactTime = new DateTime(2019, 7, 7, 1, 1, 1, 1),
                Timestamp = new DateTime(2019, 7, 7, 1, 1, 1, 1)
            };
            orderResponseList.Add(orderResponse);

            orderResponse = new BitMEX.Model.OrderResponse()
            {
                OrderId = "a758f23f-f767-739c-891f-3dff5e6d4558",
                ClOrdId = "a758f23f-f767-739c-891f-3dff5e6d4558",
                OrderQty = 1000,
                Price = 9103.5,
                LeavesQty = 0,
                CumQty = 1000,
                AvgPx = 9103.5,

                OrdType = "Limit",
                OrdStatus = "Filled",
                ExecInst = "",
                TimeInForce = "ImmediateOrCancel",

                Account = 51091,
                Symbol = "XBTUSD",
                Side = "Buy",
                StopPx = null,
                PegOffsetValue = null,
                PegPriceType = "",
                Currency = "USD",
                SettlCurrency = "XBt",
                ContingencyType = "",
                ExDestination = "XBME",
                Triggered = "",
                TransactTime = new DateTime(2019, 7, 7, 1, 1, 1, 1),
                Timestamp = new DateTime(2019, 7, 7, 1, 1, 1, 1)
            };
            orderResponseList.Add(orderResponse);

            calcBox.SetNewPosition(orderResponseList);

            #endregion Set new position 1

            // Get next step and assert...
            var nextStep = calcBox.GetNextAction();

            // Assert
            //Assert.AreEqual(1, nextStep.PositionIndex);
            //Assert.AreEqual(300, nextStep.ReversePrice);
            //Assert.AreEqual("123", nextStep.ReverseVolume);
            //Assert.AreEqual(1, nextStep.TPPrice);
            //Assert.AreEqual(1, nextStep.TPVolumeBuy);
            //Assert.AreEqual(1, nextStep.TPVolumeSell);

            #region Set new position 2
            orderResponseList = new List<BitMEX.Model.OrderResponse>();

            orderResponse = new BitMEX.Model.OrderResponse()
            {
                OrderId = "a758f23f-f767-739c-891f-3dff5e6d4558",
                ClOrdId = "a758f23f-f767-739c-891f-3dff5e6d4558",
                ClOrdLinkId = "",
                Account = 51091,
                Symbol = "XBTUSD",
                Side = "Sell",
                OrderQty = 1000,
                Price = 9103.5,
                DisplayQty = null,
                StopPx = null,
                PegOffsetValue = null,
                PegPriceType = "",
                Currency = "USD",
                SettlCurrency = "XBt",
                OrdType = "Market",
                TimeInForce = "ImmediateOrCancel",
                ExecInst = "",
                ContingencyType = "",
                ExDestination = "XBME",
                OrdStatus = "Filled",
                Triggered = "",
                LeavesQty = 0,
                CumQty = 1000,
                AvgPx = 9103.5,
                TransactTime = new DateTime(2019, 7, 7, 1, 2, 1, 1),
                Timestamp = new DateTime(2019, 7, 7, 1, 2, 1, 1)
            };
            orderResponseList.Add(orderResponse);

            calcBox.SetNewPosition(orderResponseList);

            #endregion Set new position 2

            // Get next step and assert...
            nextStep = calcBox.GetNextAction();

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