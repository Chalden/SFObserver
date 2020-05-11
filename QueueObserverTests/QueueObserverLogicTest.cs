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
using System.Fabric.Health;

namespace QueueObserverTests
{
    [TestClass]
    public class QueueObserverLogicTest
    {
        [TestMethod]
        public void WarningStateIfImpossibleAccessQueue()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();
            var wrongQueueName = "Wrong queue name";
            var cancellationToken = new CancellationToken(false);

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadQueueName()).Returns(wrongQueueName);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.OpenQueue(wrongQueueName)).Throws(new Exception());
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Warning));
            
            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            logic.ObserveAsync(cancellationToken);

            var mockLogic = new Mock<IQueueObserverLogic>();

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.OpenQueue(wrongQueueName), Times.Once());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Warning), Times.Once());
            mockLogic.Verify(QueueObserverLogic => QueueObserverLogic.ReportAsync(cancellationToken), Times.Never());
        }

        [TestMethod]
        public void WarningStateIfImpossibleQueueLengthRecovery()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();
            int? nullLength = null;
            var cancellationToken = new CancellationToken(false);

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(nullLength);

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            logic.ReportAsync(cancellationToken);

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength(), Times.Once());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Warning), Times.Once());
        }

        [TestMethod]
        public void WarningStateIfEmptyQueueLength()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();
            var length = 0;
            var cancellationToken = new CancellationToken(false);

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(length);

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            logic.ReportAsync(cancellationToken);

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength(), Times.Once());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Warning), Times.Once());
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
