namespace FluentEmailer.Core.Models;

public class SendResponse
{
    public string MessageId { get; set; } = string.Empty;
    public IList<string> ErrorMessages { get; } = new List<string>();

    public bool Successful => !ErrorMessages.Any();
}

public class SendResponse<T> : SendResponse
    where T : class
{
    public T Data { get; set; } = default!;
}
