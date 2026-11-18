using Domain.Enums;
using Domain.UseCases.Shipping;
using Unit.Helper;

namespace Unit.Domain
{
    [Trait("Category", "Unit")]
    [Trait("Component", "ShippingStage")]
    public class ShippingStageTests
    {
        private readonly ShippingStage _stage;

        public ShippingStageTests()
        {
            _stage = new ShippingStage();
        }

        [Fact]
        [Trait("Feature", "Shipping")]
        public void ProcessShipsPaidOrder()
        {
            var order = DomainTestHelper.CreateOrder();

            order.setStatus(OrderStatus.Paid);

            var result = _stage.Process(order);

            Assert.Equal(EnumStatus.SUCESSO, result.Status);
            Assert.Same(order, result.SuccessObject);
            Assert.Equal(OrderStatus.Shipped, order.status);
        }

        [Theory]
        [InlineData(OrderStatus.Validated)]
        [InlineData(OrderStatus.Invalid)]
        [InlineData(OrderStatus.Shipped)]
        [InlineData(OrderStatus.Failed)]
        [Trait("Feature", "StatusValidation")]
        public void ProcessRejectsOrderThatIsNotPaid(
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
