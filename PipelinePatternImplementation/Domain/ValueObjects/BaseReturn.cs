using Domain.Enums;
using System.Text.Json.Serialization;

namespace PipelinePatternImplementation.Domain.ValueObjects;

public struct BaseReturn<T>
{
    public EnumStatus Status { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public T? SuccessObject { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public BaseError? ErrorObject { get; set; }

    public BaseReturn<T> Sucesso(T successObject)
    {
        Status = EnumStatus.SUCESSO;
        SuccessObject = successObject;
        ErrorObject = null;

        return this;
    }

    public BaseReturn<T> ErroNegocio(BaseError error)
    {
        return Erro(EnumStatus.NEGOCIO, error);
    }

    public BaseReturn<T> ErroSistema(BaseError error)
    {
        return Erro(EnumStatus.SISTEMA, error);
    }

    public BaseReturn<T> Erro(EnumStatus status, BaseError error)
    {
        Status = status;
        SuccessObject = default;
        ErrorObject = error;

        return this;
    }
}