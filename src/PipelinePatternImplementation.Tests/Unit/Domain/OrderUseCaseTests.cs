using Domain.Enums;
using Domain.Models.Order;
using Domain.Ports.InBound;
using Domain.UseCases.OrderUseCase;
using Moq;
using Unit.Helper;

namespace Unit.Domain
{
    [Trait("Category", "Unit")]
    [Trait("Component", "OrderUseCase")]
    public class OrderUseCaseTests
    {
        private readonly Mock<IOrderProcessingPipeline> _pipeline;
        private readonly OrderUseCase _useCase;

        public OrderUseCaseTests()
        {
            _pipeline = new Mock<IOrderProcessingPipeline>();

            _useCase = new OrderUseCase(_pipeline.Object);
        }

        [Fact]
        [Trait("Feature", "Construction")]
        public void CanConstruct()
        {
            Assert.NotNull(_useCase);
        }

        [Fact]
        [Trait("Feature", "ArgumentValidation")]
        public void CannotConstructWithNullPipeline()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new OrderUseCase(null!));
        }

        [Fact]
        [Trait("Feature", "Execution")]
        public async Task ExecutarAsyncReturnsPipelineSuccess()
        {
            var order = DomainTestHelper.CreateOrder();

            var expected = DomainTestHelper.Success(order);

            _pipeline
                .Setup(x => x.Execute(order))
                .Returns(expected);

            var result = await _useCase.ExecutarAsync(order);

            Assert.Equal(expected, result);
            Assert.Equal(EnumStatus.SUCESSO, result.Status);
            Assert.Same(order, result.SuccessObject);

            _pipeline.Verify(
                x => x.Execute(order),
                Times.Once);

            _pipeline.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        [Trait("Feature", "ErrorPropagation")]
        public async Task ExecutarAsyncReturnsPipelineError(
            bool systemError)
        {
            var order = DomainTestHelper.CreateOrder();

            var expected = systemError
                ? DomainTestHelper.SystemError()
                : DomainTestHelper.BusinessError();

            _pipeline
                .Setup(x => x.Execute(order))
                .Returns(expected);

            var result = await _useCase.ExecutarAsync(order);

            Assert.Equal(expected, result);
            Assert.Equal(expected.ErrorObject, result.ErrorObject);

            _pipeline.Verify(
                x => x.Execute(order),
                Times.Once);
        }

        [Fact]
        [Trait("Feature", "ExceptionHandling")]
        public async Task ExecutarAsyncReturnsSystemErrorWhenPipelineThrows()
        {
            var order = DomainTestHelper.CreateOrder();

            _pipeline
                .Setup(x => x.Execute(order))
                .Throws(new InvalidOperationException(
                    "Falha simulada no pipeline."));

            var result = await _useCase.ExecutarAsync(order);

            Assert.Equal(EnumStatus.SISTEMA, result.Status);
            Assert.NotNull(result.ErrorObject);

            _pipeline.Verify(
                x => x.Execute(order),
                Times.Once);
        }

        [Fact]
        [Trait("Feature", "ArgumentValidation")]
        public async Task ExecutarAsyncReturnsSystemErrorWhenOrderIsNull()
        {
            var result = await _useCase.ExecutarAsync(null!);

            Assert.Equal(EnumStatus.SISTEMA, result.Status);
            Assert.NotNull(result.ErrorObject);

            _pipeline.Verify(
                x => x.Execute(It.IsAny<Order>()),
                Times.Never);
        }
    }
}
