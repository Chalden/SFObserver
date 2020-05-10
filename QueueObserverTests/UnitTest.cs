using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FabricObserver.Observers.Interfaces;
using FabricObserver.Observers;
using FabricObserver.Observers.Utilities;
using System.Threading;

namespace QueueObserverTests
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void ThrowsExceptionIfCantAccessQueue()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();

            var wrongQueueName = "Wrong queue name";

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadQueueName()).Returns(wrongQueueName);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.OpenQueue(wrongQueueName)).Throws(new ArgumentException(wrongQueueName));

            var mockLogic = new Mock<IQueueObserverLogic>(mockAccessor);

            var falseToken = new CancellationToken(false);

            mockLogic.Verify(QueueObserverLogic => QueueObserverLogic.ObserveAsync(falseToken));
        }
    }
}
