using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using FabricObserver.Observers.Interfaces;
using FabricObserver.Observers;
using FabricObserver.Observers.Utilities;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Queue;
using System.Collections.Generic;
using System.Linq;

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
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.OpenQueue(mockAccessor.Object.LoadQueueName())).Throws(new Exception());

            var mockLogic = new Mock<QueueObserverLogic>(mockAccessor);

            IQueueObserverAccessor queueAccessor = mockAccessor.Object;

            var wrongQueueName = queueAccessor.LoadQueueName();

            Assert.ThrowsException<Exception>(() => queueAccessor.OpenQueue(wrongQueueName));

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.LoadQueueName(), Times.AtLeastOnce());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.OpenQueue(wrongQueueName), Times.AtLeastOnce());
        }

        [TestMethod]
        public void ImpossibleQueueLengthRecovery()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();

            int? nullLength = null;

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(nullLength);

            IQueueObserverAccessor queueAccessor = mockAccessor.Object;

            var queueLength = queueAccessor.TryGetQueueLength();

            Assert.IsFalse(queueLength.HasValue);

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength(), Times.AtLeastOnce());
        }

        [TestMethod]
        public void EmptyQueueLength()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(0);

            IQueueObserverAccessor queueAccessor = mockAccessor.Object;

            var queueLength = queueAccessor.TryGetQueueLength();

            Assert.AreEqual(queueLength, 0);

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength(), Times.AtLeastOnce());
        }

        [TestMethod]
        public void NullInsertionTimeMessage()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();

            var messagesNumber = 32;

            var message = new CloudQueueMessage("Bad message");
            IEnumerable<CloudQueueMessage> messages = new List<CloudQueueMessage>(){ message };

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.PeekMessages(messagesNumber)).Returns(messages);

            IQueueObserverAccessor queueAccessor = mockAccessor.Object;

            var peekedMessages = queueAccessor.PeekMessages(messagesNumber);
            var messageInsertionTime = peekedMessages.First().InsertionTime;

            Assert.AreEqual(messageInsertionTime, null);

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.PeekMessages(messagesNumber), Times.AtLeastOnce());
        }

        [TestMethod]
        public void HealthReportState()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadWarningLength()).Returns(3);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadCriticalLength()).Returns(5);

            IQueueObserverAccessor queueAccessor = mockAccessor.Object;

            int ok = 1, warning = 4, error = 7;

            var warningLength = queueAccessor.LoadWarningLength();
            var criticalLength = queueAccessor.LoadCriticalLength();

            Assert.IsTrue(error >= criticalLength);
            Assert.IsTrue(warning >= warningLength);
            Assert.IsFalse(ok >= criticalLength || ok >= warningLength);

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.LoadWarningLength(), Times.AtLeastOnce());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.LoadCriticalLength(), Times.AtLeastOnce());
        }
    }
}
