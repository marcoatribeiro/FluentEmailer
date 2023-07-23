namespace FluentEmailer.Mailgun.HttpHelpers;

public class ApiResponse
{
    public bool Success => !Errors.Any();
    public IList<ApiError> Errors { get; init; } = new List<ApiError>();
}

public class ApiResponse<T> : ApiResponse
{
    public T Data { get; set; } = default!;
}

public class ApiError
{
    public string ErrorCode { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string PropertyName { get; set; } = string.Empty;
}
