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
        public async Task WarningStateIfImpossibleAccessQueue()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();
            var wrongQueueName = "Wrong queue name";
            var cancellationToken = new CancellationToken(false);

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadQueueName()).Returns(wrongQueueName);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.OpenQueue(wrongQueueName)).Throws(new Exception());
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Warning));

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            await logic.ObserveAsync(cancellationToken);

            var mockLogic = new Mock<IQueueObserverLogic>();

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.OpenQueue(wrongQueueName), Times.Once());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Warning), Times.Once());
            mockLogic.Verify(QueueObserverLogic => QueueObserverLogic.ReportAsync(cancellationToken), Times.Never());
        }

        [TestMethod]
        public async Task WarningStateIfImpossibleQueueLengthRecovery()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();
            var cancellationToken = new CancellationToken(false);

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(default(int));

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            await logic.ReportAsync(cancellationToken);

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength(), Times.Once());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Warning), Times.Once());
        }

        [TestMethod]
        public async Task WarningStateIfEmptyQueueLength()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();
            var length = 0;
            var cancellationToken = new CancellationToken(false);

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(length);

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            await logic.ReportAsync(cancellationToken);

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength(), Times.Once());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Warning), Times.Once());
        }

        [TestMethod]
        public async Task WarningStateIfImpossibleMessageAttributesRecovery()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();
            var messagesNumber = 32;
            var cancellationToken = new CancellationToken(false);
            var message = new CloudQueueMessage("Bad message");
            IEnumerable<CloudQueueMessage> messages = new List<CloudQueueMessage>() { message };

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(messages.Count());
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.PeekMessages(messagesNumber)).Returns(messages);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Warning));

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            await logic.ReportAsync(cancellationToken);

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength(), Times.Once());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.PeekMessages(messagesNumber), Times.Once());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Warning), Times.Once());
        }

        [TestMethod]
        public async Task SendWarningHealthState()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();
            var messagesNumber = 32;
            var cancellationToken = new CancellationToken(false);
            IEnumerable<CloudQueueMessage> messages = new List<CloudQueueMessage>() { };

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadWarningLength()).Returns(3);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(4);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadCriticalLength()).Returns(5);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.PeekMessages(messagesNumber)).Returns(messages);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Warning));

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            await logic.ObserveAsync(cancellationToken);

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.LoadWarningLength(), Times.Once());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength(), Times.Once());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.LoadCriticalLength(), Times.Once());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Warning), Times.Once());
        }

        [TestMethod]
        public async Task SendErrorHealthState()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();
            var messagesNumber = 32;
            var cancellationToken = new CancellationToken(false);
            IEnumerable<CloudQueueMessage> messages = new List<CloudQueueMessage>() { };

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadWarningLength()).Returns(3);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(6);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadCriticalLength()).Returns(5);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.PeekMessages(messagesNumber)).Returns(messages);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Error));

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            await logic.ObserveAsync(cancellationToken);

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.LoadWarningLength(), Times.Once());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength(), Times.Once());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.LoadCriticalLength(), Times.Once());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Error), Times.Once());
        }

        [TestMethod]
        public async Task SendOkHealthState()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();
            var messagesNumber = 32;
            var cancellationToken = new CancellationToken(false);
            IEnumerable<CloudQueueMessage> messages = new List<CloudQueueMessage>() { };

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadWarningLength()).Returns(3);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(2);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadCriticalLength()).Returns(5);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.PeekMessages(messagesNumber)).Returns(messages);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Ok));

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            await logic.ObserveAsync(cancellationToken);

            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.LoadWarningLength(), Times.Once());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength(), Times.Once());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.LoadCriticalLength(), Times.Once());
            mockAccessor.Verify(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Ok), Times.Once());
        }

        [TestMethod]
        public async Task ExitFunctionIfCancellationRequested()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();
            var cancellationToken = new CancellationToken(true);

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            await logic.ReportAsync(cancellationToken);

            mockAccessor.VerifyNoOtherCalls();
        }
    }
}
