using Domain.Enums;
using Domain.UseCases.Validation;
using Unit.Helper;

namespace Unit.Domain
{
    [Trait("Category", "Unit")]
    [Trait("Component", "ValidationStage")]
    public class ValidationStageTests
    {
        private readonly ValidationStage _stage;

        public ValidationStageTests()
        {
            _stage = new ValidationStage();
        }

        [Fact]
        [Trait("Feature", "Validation")]
        public void ProcessValidatesOrderWithPositiveAmount()
        {
            var order = DomainTestHelper.CreateOrder(100m);

            var result = _stage.Process(order);

            Assert.Equal(EnumStatus.SUCESSO, result.Status);
            Assert.Same(order, result.SuccessObject);
            Assert.Equal(OrderStatus.Validated, order.status);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        [InlineData(-100)]
        [Trait("Feature", "Validation")]
        public void ProcessRejectsOrderWithNonPositiveAmount(
            int amount)
        {
            var order = DomainTestHelper.CreateOrder(amount);

            var result = _stage.Process(order);

            Assert.Equal(EnumStatus.NEGOCIO, result.Status);
            Assert.NotNull(result.ErrorObject);
            Assert.Equal(OrderStatus.Invalid, order.status);
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
