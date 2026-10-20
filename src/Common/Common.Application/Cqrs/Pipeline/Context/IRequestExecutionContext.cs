namespace Common.Application.Cqrs.Pipeline.Context;
internal interface IRequestExecutionContext
{
    string? ServedFrom { get; }
    void MarkServedFrom(string source);
}