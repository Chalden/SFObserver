using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FabricObserver.Observers.Interfaces;
using FabricObserver.Observers;

namespace QueueObserverTests
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void TestMethod()
        {
            var MockQOA = new Mock<IQueueObserverAccessor>();
            MockQOA.Setup(QueueObserverLogic => QueueObserverLogic.LoadWarningLength()).Returns(2);
            MockQOA.Setup(QueueObserverLogic => QueueObserverLogic.LoadCriticalLength()).Returns(3);
            MockQOA.Setup(QueueObserverLogic => QueueObserverLogic.LoadMaxAcceptableDequeueCount()).Returns(3);
        }
    }
}
