using Domain.Enums;
using PipelinePatternImplementation.Domain.ValueObjects;

namespace Domain.ValueObjects
{
    public abstract class BaseUseCase
    {
        protected static BaseReturn<T> ErroNegocio<T>(
            int codErro,
            string mensagem)
        {
            return new BaseReturn<T>().ErroNegocio(
                new BaseError(
                    EnumStatus.NEGOCIO,
                    codErro,
                    mensagem));
        }

        protected static BaseReturn<T> ErroSistema<T>(
            Exception ex,
            int codErro)
        {
            return new BaseReturn<T>().ErroSistema(
                new BaseError(
                    EnumStatus.SISTEMA,
                    codErro,
                    ex.Message));
        }
    }
}
