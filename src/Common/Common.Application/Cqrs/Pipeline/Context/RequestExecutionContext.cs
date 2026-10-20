namespace Common.Application.Cqrs.Pipeline.Context;
internal sealed class RequestExecutionContext : IRequestExecutionContext
{
    public string? ServedFrom { get; private set; }
    public void MarkServedFrom(string source) => ServedFrom = source;
}