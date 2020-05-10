using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FabricObserver.Observers.Interfaces;
using FabricObserver.Observers;
using FabricObserver.Observers.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace QueueObserverTests
{
    [TestClass]
    public class UnitTest
    {
        [TestMethod]
        public void ThrowsExceptionIfCantAccessQueue()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadQueueName()).Returns("Wrong queue name");
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.OpenQueue(QueueObserverAccessor.LoadQueueName())).Throws(new Exception());

            var mockLogic = new Mock<IQueueObserverLogic>(mockAccessor);

            IQueueObserverAccessor queueAccessor = mockAccessor.Object;
            IQueueObserverLogic logic = new QueueObserverLogic(queueAccessor);

            var falseToken = new CancellationToken(false);
            var wrongQueueName = queueAccessor.LoadQueueName();

            Assert.ThrowsException<Exception>(() => queueAccessor.OpenQueue(wrongQueueName));

            logic.ObserveAsync(falseToken);

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.LoadQueueName(), Times.AtLeastOnce());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.OpenQueue(wrongQueueName), Times.AtLeastOnce());
            mockLogic.Verify(QueueObserverLogic => QueueObserverLogic.ObserveAsync(falseToken), Times.AtLeastOnce());
        }
    }
}
