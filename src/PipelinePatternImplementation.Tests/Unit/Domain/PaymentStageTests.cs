using Domain.Enums;
using Domain.UseCases.Payment;
using Unit.Helper;

namespace Unit.Domain
{
    [Trait("Category", "Unit")]
    [Trait("Component", "PaymentStage")]
    public class PaymentStageTests
    {
        private readonly PaymentStage _stage;

        public PaymentStageTests()
        {
            _stage = new PaymentStage();
        }

        [Fact]
        [Trait("Feature", "Payment")]
        public void ProcessPaysValidatedOrder()
        {
            var order = DomainTestHelper.CreateOrder();

            order.setStatus(OrderStatus.Validated);

            var result = _stage.Process(order);

            Assert.Equal(EnumStatus.SUCESSO, result.Status);
            Assert.Same(order, result.SuccessObject);
            Assert.Equal(OrderStatus.Paid, order.status);
        }

        [Theory]
        [InlineData(OrderStatus.Invalid)]
        [InlineData(OrderStatus.Paid)]
        [InlineData(OrderStatus.Shipped)]
        [InlineData(OrderStatus.Failed)]
        [Trait("Feature", "StatusValidation")]
        public void ProcessRejectsOrderThatIsNotValidated(
            OrderStatus status)
        {
            var order = DomainTestHelper.CreateOrder();

            order.setStatus(status);

            var result = _stage.Process(order);

            Assert.Equal(EnumStatus.NEGOCIO, result.Status);
            Assert.NotNull(result.ErrorObject);
            Assert.Equal(OrderStatus.Failed, order.status);
        }

        [Fact]
        [Trait("Feature", "ArgumentValidation")]
        public void ProcessReturnsSystemErrorWhenOrderIsNull()
        {
            var result = _stage.Process(null!);

            Assert.Equal(EnumStatus.SISTEMA, result.Status);
            Assert.NotNull(result.ErrorObject);
        }
    }
}
