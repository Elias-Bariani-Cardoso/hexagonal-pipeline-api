using Domain.Enums;

namespace PipelinePatternImplementation.Domain.ValueObjects;

public struct BaseError
{
    public EnumStatus? tipoErro { get; init; }
    public int? codErro { get; init; }
    public string? msgErro { get; init; }

    public BaseError(
        EnumStatus tipoErro,
        int codErro,
        string? msgErro)
    {
        this.tipoErro = tipoErro;
        this.codErro = codErro;
        this.msgErro = msgErro;
    }

    public BaseError(Error error)
    {
        tipoErro = (EnumStatus)int.Parse(error.tipoErro!);
        codErro = int.Parse(error.codErro!);
        msgErro = error.msgErro;
    }
}