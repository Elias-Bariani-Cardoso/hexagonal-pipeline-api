using Domain.Enums;
using Domain.Models.Order;
using Domain.Ports.InBound;
using Domain.Services;
using Moq;
using Unit.Helper;

namespace Unit.Domain
{
    [Trait("Category", "Unit")]
    [Trait("Component", "OrderProcessingPipeline")]
    public class OrderProcessingPipelineTests
    {
        [Fact]
        [Trait("Feature", "ArgumentValidation")]
        public void CannotConstructWithNullServiceProvider()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new OrderProcessingPipeline(null!));
        }

        [Fact]
        [Trait("Feature", "DependencyResolution")]
        public void CannotConstructWithoutRegisteredStages()
        {
            var serviceProvider = new Mock<IServiceProvider>();

            Assert.Throws<InvalidOperationException>(() =>
                new OrderProcessingPipeline(serviceProvider.Object));
        }

        [Fact]
        [Trait("Feature", "Execution")]
        public void ExecuteReturnsSuccessWhenThereAreNoStages()
        {
            var order = DomainTestHelper.CreateOrder();

            var pipeline = CreatePipeline();

            var result = pipeline.Execute(order);

            Assert.Equal(EnumStatus.SUCESSO, result.Status);
            Assert.Same(order, result.SuccessObject);
        }

        [Fact]
        [Trait("Feature", "ExecutionOrder")]
        public void ExecuteProcessesStagesInExpectedOrder()
        {
            var order = DomainTestHelper.CreateOrder();

            var validation = new Mock<IOrderProcessingStage>();
            var payment = new Mock<IOrderProcessingStage>();
            var shipping = new Mock<IOrderProcessingStage>();

            var executionOrder = new List<string>();

            validation
                .Setup(x => x.Process(order))
                .Callback(() => executionOrder.Add("Validation"))
                .Returns(DomainTestHelper.Success(order));

            payment
                .Setup(x => x.Process(order))
                .Callback(() => executionOrder.Add("Payment"))
                .Returns(DomainTestHelper.Success(order));

            shipping
                .Setup(x => x.Process(order))
                .Callback(() => executionOrder.Add("Shipping"))
                .Returns(DomainTestHelper.Success(order));

            var pipeline = CreatePipeline(
                validation.Object,
                payment.Object,
                shipping.Object);

            var result = pipeline.Execute(order);

            Assert.Equal(EnumStatus.SUCESSO, result.Status);
            Assert.Same(order, result.SuccessObject);

            Assert.Equal(
                new[] { "Validation", "Payment", "Shipping" },
                executionOrder);

            validation.Verify(x => x.Process(order), Times.Once);
            payment.Verify(x => x.Process(order), Times.Once);
            shipping.Verify(x => x.Process(order), Times.Once);
        }

        [Fact]
        [Trait("Feature", "DataFlow")]
        public void ExecutePassesReturnedOrderToNextStage()
        {
            var originalOrder = DomainTestHelper.CreateOrder(100m);
            var updatedOrder = DomainTestHelper.CreateOrder(200m);
            var finalOrder = DomainTestHelper.CreateOrder(300m);

            var firstStage = new Mock<IOrderProcessingStage>();
            var secondStage = new Mock<IOrderProcessingStage>();

            firstStage
                .Setup(x => x.Process(originalOrder))
                .Returns(DomainTestHelper.Success(updatedOrder));

            secondStage
                .Setup(x => x.Process(updatedOrder))
                .Returns(DomainTestHelper.Success(finalOrder));

            var pipeline = CreatePipeline(
                firstStage.Object,
                secondStage.Object);

            var result = pipeline.Execute(originalOrder);

            Assert.Equal(EnumStatus.SUCESSO, result.Status);
            Assert.Same(finalOrder, result.SuccessObject);

            firstStage.Verify(
                x => x.Process(originalOrder),
                Times.Once);

            secondStage.Verify(
                x => x.Process(updatedOrder),
                Times.Once);

            secondStage.Verify(
                x => x.Process(originalOrder),
                Times.Never);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [Trait("Feature", "ShortCircuit")]
        public void ExecuteStopsAndReturnsStageError(
            bool systemError)
        {
            var order = DomainTestHelper.CreateOrder();

            var firstStage = new Mock<IOrderProcessingStage>();
            var failingStage = new Mock<IOrderProcessingStage>();
            var lastStage = new Mock<IOrderProcessingStage>();

            var expected = systemError
                ? DomainTestHelper.SystemError()
                : DomainTestHelper.BusinessError();

            firstStage
                .Setup(x => x.Process(order))
                .Returns(DomainTestHelper.Success(order));

            failingStage
                .Setup(x => x.Process(order))
                .Returns(expected);

            var pipeline = CreatePipeline(
                firstStage.Object,
                failingStage.Object,
                lastStage.Object);

            var result = pipeline.Execute(order);

            Assert.Equal(expected, result);

            firstStage.Verify(
                x => x.Process(order),
                Times.Once);

            failingStage.Verify(
                x => x.Process(order),
                Times.Once);

            lastStage.Verify(
                x => x.Process(It.IsAny<Order>()),
                Times.Never);
        }

        [Fact]
        [Trait("Feature", "ExceptionHandling")]
        public void ExecuteReturnsSystemErrorWhenStageThrows()
        {
            var order = DomainTestHelper.CreateOrder();

            var failingStage = new Mock<IOrderProcessingStage>();
            var nextStage = new Mock<IOrderProcessingStage>();

            failingStage
                .Setup(x => x.Process(order))
                .Throws(new InvalidOperationException(
                    "Falha simulada na etapa."));

            var pipeline = CreatePipeline(
                failingStage.Object,
                nextStage.Object);

            var result = pipeline.Execute(order);

            Assert.Equal(EnumStatus.SISTEMA, result.Status);
            Assert.NotNull(result.ErrorObject);

            nextStage.Verify(
                x => x.Process(It.IsAny<Order>()),
                Times.Never);
        }

        [Fact]
        [Trait("Feature", "ArgumentValidation")]
        public void ExecuteReturnsSystemErrorWhenOrderIsNull()
        {
            var stage = new Mock<IOrderProcessingStage>();

            var pipeline = CreatePipeline(stage.Object);

            var result = pipeline.Execute(null!);

            Assert.Equal(EnumStatus.SISTEMA, result.Status);
            Assert.NotNull(result.ErrorObject);

            stage.Verify(
                x => x.Process(It.IsAny<Order>()),
                Times.Never);
        }

        private static OrderProcessingPipeline CreatePipeline(
            params IOrderProcessingStage[] stages)
        {
            var serviceProvider = new Mock<IServiceProvider>();

            serviceProvider
                .Setup(x => x.GetService(
                    typeof(IEnumerable<IOrderProcessingStage>)))
                .Returns(stages);

            return new OrderProcessingPipeline(
                serviceProvider.Object);
        }
    }
}
