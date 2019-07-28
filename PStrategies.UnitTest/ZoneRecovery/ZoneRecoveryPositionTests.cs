using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PStrategies.ZoneRecovery;

namespace PStrategies.UnitTest.ZoneRecovery
{
    [TestClass]
    public class ZoneRecoveryPositionTests
    {
        [TestMethod]
        public void ZoneRecoveryPosition_PriceAndQtyInInit_ReturnsCorrectValues()
        {
            // Arrange
            var ZoneRecoveryPosition = new ZoneRecoveryPosition("123", 0.5, 1, 1000, 100);
            ZoneRecoveryPosition.AddToPosition(1010, 100);
            ZoneRecoveryPosition.AddToPosition(1020, 100);
            
            //Act 
            var avgPrice  = ZoneRecoveryPosition.AVGPrice;
            var totQty = ZoneRecoveryPosition.TotalQty;
            var ordId = ZoneRecoveryPosition.OrderID;
            var posIndex = ZoneRecoveryPosition.PositionIndex;

            // Assert
            Assert.AreEqual(1010, avgPrice);
            Assert.AreEqual(300, totQty);
            Assert.AreEqual("123", ordId);
            Assert.AreEqual(1, posIndex);
        }

        [TestMethod]
        public void ZoneRecoveryPosition_WithoutPriceAndQtyInit_ReturnsCorrectValues()
        {
            // Arrange
            var ZoneRecoveryPosition = new ZoneRecoveryPosition("123", 0.5, 1);
            ZoneRecoveryPosition.AddToPosition(1000, 100);
            ZoneRecoveryPosition.AddToPosition(1020, 50);

            //Act 
            var avgPrice = ZoneRecoveryPosition.AVGPrice;
            var totQty = ZoneRecoveryPosition.TotalQty;
            var ordId = ZoneRecoveryPosition.OrderID;
            var posIndex = ZoneRecoveryPosition.PositionIndex;

            // Assert
            Assert.AreEqual(1006.5, avgPrice);
            Assert.AreEqual(150, totQty);
            Assert.AreEqual("123", ordId);
            Assert.AreEqual(1, posIndex);
        }

        [TestMethod]
        public void CalculateAveragePrice_CorrectInputSpecified_ReturnsCorrectValues()
        {
            // Act
            var avgPrice1 = ZoneRecoveryPosition.CalculateAveragePrice(1000, 1100, 100, 100, 0.5);
            var avgPrice2 = ZoneRecoveryPosition.CalculateAveragePrice(985, 1027, 89, 101, 0.5);
            var avgPrice3 = ZoneRecoveryPosition.CalculateAveragePrice(985, 1027, 89, 101, 0.1);
            var avgPrice4 = ZoneRecoveryPosition.CalculateAveragePrice(985, 1027, 89, 101, 0.0001);

            // Assert
            Assert.AreEqual(1050, avgPrice1);
            Assert.AreEqual(1007.5, avgPrice2);
            Assert.AreEqual(1007.3, avgPrice3);
            Assert.AreEqual(1007.3263, avgPrice4);
        }
    }
}
