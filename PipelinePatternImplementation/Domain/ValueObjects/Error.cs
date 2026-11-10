namespace PipelinePatternImplementation.Domain.ValueObjects;

public struct Error
{
    public string? tipoErro { get; init; }
    public string? codErro { get; init; }
    public string? msgErro { get; init; }
}