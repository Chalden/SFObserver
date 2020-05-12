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

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadQueueName()).Returns(wrongQueueName);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.OpenQueue(wrongQueueName)).Throws(new Exception());
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Warning)).Verifiable();

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            await logic.ObserveAsync(new CancellationToken(false));

            mockAccessor.VerifyAll();
        }

        [TestMethod]
        public async Task WarningStateIfImpossibleQueueLengthRecovery()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(default(int?));
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Warning)).Verifiable();

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            await logic.ReportAsync(new CancellationToken(false));

            mockAccessor.VerifyAll();
        }

        [TestMethod]
        public async Task OkStateIfEmptyQueue()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(0);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Ok)).Verifiable();

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            await logic.ReportAsync(new CancellationToken(false));

            mockAccessor.VerifyAll();
        }

        [TestMethod]
        public async Task WarningStateIfImpossibleMessageAttributesRecovery()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();
            var message = new CloudQueueMessage("Bad message");
            IEnumerable<CloudQueueMessage> messages = new List<CloudQueueMessage>() { message };

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(messages.Count());
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.PeekMessages(32)).Returns(messages);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Warning)).Verifiable();

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            await logic.ReportAsync(new CancellationToken(false));

            mockAccessor.VerifyAll();        
        }

        [TestMethod]
        public async Task SendWarningHealthState()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();
            IEnumerable<CloudQueueMessage> messages = new List<CloudQueueMessage>() { };

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadWarningLength()).Returns(3);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(4);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadCriticalLength()).Returns(5);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.PeekMessages(32)).Returns(messages);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Warning)).Verifiable();

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            await logic.ObserveAsync(new CancellationToken(false));

            mockAccessor.VerifyAll();
        }

        [TestMethod]
        public async Task SendErrorHealthState()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();
            IEnumerable<CloudQueueMessage> messages = new List<CloudQueueMessage>() { };

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadWarningLength()).Returns(3);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(6);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadCriticalLength()).Returns(5);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.PeekMessages(32)).Returns(messages);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Error)).Verifiable();

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            await logic.ObserveAsync(new CancellationToken(false));

            mockAccessor.VerifyAll();
        }

        [TestMethod]
        public async Task SendOkHealthState()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();
            IEnumerable<CloudQueueMessage> messages = new List<CloudQueueMessage>() { };

            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadWarningLength()).Returns(3);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.TryGetQueueLength()).Returns(2);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.LoadCriticalLength()).Returns(5);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.PeekMessages(32)).Returns(messages);
            mockAccessor.Setup(QueueObserverAccessor => QueueObserverAccessor.SendReport(It.IsAny<string>(), HealthState.Ok)).Verifiable();

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            await logic.ObserveAsync(new CancellationToken(false));

            mockAccessor.VerifyAll();
        }

        [TestMethod]
        public async Task ExitFunctionIfCancellationRequested()
        {
            var mockAccessor = new Mock<IQueueObserverAccessor>();

            IQueueObserverLogic logic = new QueueObserverLogic(mockAccessor.Object);
            await logic.ReportAsync(new CancellationToken(true));

            mockAccessor.VerifyNoOtherCalls();
        }
    }
}
