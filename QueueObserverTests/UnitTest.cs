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

        public void ImpossibleQueueLengthRecovery()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();

            int? nullLength = null;

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(nullLength);
            
            var mockLogic = new Mock<QueueObserverLogic>(mockAccessor);

            var falseToken = new CancellationToken(false);

            mockLogic.Setup(QueueObserverLogic => QueueObserverLogic.ReportAsync(falseToken)).Returns(Task.CompletedTask);

            IQueueObserverAccessor queueAccessor = mockAccessor.Object;
            IQueueObserverLogic logic = new QueueObserverLogic(queueAccessor);

            var queueLength = queueAccessor.TryGetQueueLength();
            var task = logic.ReportAsync(falseToken);

            Assert.IsFalse(queueLength.HasValue);
            Assert.Equals(task,Task.CompletedTask);

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength(), Times.AtLeastOnce());
            mockLogic.Verify(QueueObserverLogic => QueueObserverLogic.ReportAsync(falseToken), Times.AtLeastOnce());
        }

        public void EmptyQueueLength()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(0);

            var mockLogic = new Mock<QueueObserverLogic>(mockAccessor);

            var falseToken = new CancellationToken(false);

            mockLogic.Setup(QueueObserverLogic => QueueObserverLogic.ReportAsync(falseToken)).Returns(Task.CompletedTask);

            IQueueObserverAccessor queueAccessor = mockAccessor.Object;
            IQueueObserverLogic logic = new QueueObserverLogic(queueAccessor);

            var queueLength = queueAccessor.TryGetQueueLength();
            var task = logic.ReportAsync(falseToken);

            Assert.Equals(queueLength, 0);
            Assert.Equals(task, Task.CompletedTask);

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength(), Times.AtLeastOnce());
            mockLogic.Verify(QueueObserverLogic => QueueObserverLogic.ReportAsync(falseToken), Times.AtLeastOnce());
        }

        public void NullInsertionTimeMessage()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();

            var messagesNumber = 32;

            var message = new CloudQueueMessage("Bad message");
            IEnumerable<CloudQueueMessage> messages = new List<CloudQueueMessage>(){ message };

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.PeekMessages(messagesNumber)).Returns(messages);

            var mockLogic = new Mock<QueueObserverLogic>(mockAccessor);

            var falseToken = new CancellationToken(false);

            mockLogic.Setup(QueueObserverLogic => QueueObserverLogic.ReportAsync(falseToken)).Returns(Task.CompletedTask);

            IQueueObserverAccessor queueAccessor = mockAccessor.Object;
            IQueueObserverLogic logic = new QueueObserverLogic(queueAccessor);

            var peekedMessages = queueAccessor.PeekMessages(messagesNumber);
            var messageInsertionTime = peekedMessages.First().InsertionTime;
     
            var task = logic.ReportAsync(falseToken);

            Assert.Equals(messageInsertionTime, null);
            Assert.Equals(task, Task.CompletedTask);

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.PeekMessages(messagesNumber), Times.AtLeastOnce());
            mockLogic.Verify(QueueObserverLogic => QueueObserverLogic.ReportAsync(falseToken), Times.AtLeastOnce());
        }
    }
}
