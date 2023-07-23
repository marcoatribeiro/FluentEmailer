namespace FluentEmailer.Mailgun.HttpHelpers;

public class HttpClientHelpers
{
    public static HttpContent GetPostBody(IEnumerable<KeyValuePair<string, string>> parameters)
    {
        var formatted = parameters.Select(x => x.Key + "=" + x.Value);
        return new StringContent(string.Join("&", formatted), Encoding.UTF8, "application/x-www-form-urlencoded");
    }

    public static HttpContent GetJsonBody(object value) 
        => new StringContent(JsonSerializer.Serialize(value), Encoding.UTF8, "application/json");

    public static HttpContent GetMultipartFormDataContentBody(IEnumerable<KeyValuePair<string, string>> parameters, IEnumerable<HttpFile> files)
    {
        var mpContent = new MultipartFormDataContent();

        parameters.ForEach(p =>
        {
            mpContent.Add(new StringContent(p.Value), p.Key);
        });

        files.ForEach(file =>
        {
            using var memoryStream = new MemoryStream();
            file.Data.CopyTo(memoryStream);
            mpContent.Add(new ByteArrayContent(memoryStream.ToArray()), file.ParameterName, file.Filename);
        });

        return mpContent;
    }
}

public static class HttpClientExtensions
{
    public static async Task<ApiResponse<T>> Get<T>(this HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        var qr = await QuickResponse<T>.FromMessage(response);
        return qr.ToApiResponse();
    }

    public static async Task<ApiResponse> GetFile(this HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        var qr = await QuickFile.FromMessage(response);
        return qr.ToApiResponse();
    }

    public static async Task<ApiResponse<T>> Post<T>(this HttpClient client, string url, IEnumerable<KeyValuePair<string, string>> parameters)
    {
        var response = await client.PostAsync(url, HttpClientHelpers.GetPostBody(parameters));
        var qr = await QuickResponse<T>.FromMessage(response);
        return qr.ToApiResponse();
    }

    public static async Task<ApiResponse<T>> Post<T>(this HttpClient client, string url, object data)
    {
        var response = await client.PostAsync(url, HttpClientHelpers.GetJsonBody(data));
        var qr = await QuickResponse<T>.FromMessage(response);
        return qr.ToApiResponse();
    }

    public static async Task<ApiResponse<T>> PostMultipart<T>(this HttpClient client, string url, IEnumerable<KeyValuePair<string, string>> parameters, IEnumerable<HttpFile> files)
    {
        var response = await client.PostAsync(url, HttpClientHelpers.GetMultipartFormDataContentBody(parameters, files)).ConfigureAwait(false);
        var qr = await QuickResponse<T>.FromMessage(response);
        return qr.ToApiResponse();
    }

    public static async Task<ApiResponse> Delete(this HttpClient client, string url)
    {
        var response = await client.DeleteAsync(url);
        var qr = await QuickResponse.FromMessage(response);
        return qr.ToApiResponse();
    }
}

public class QuickResponse
{
    public HttpResponseMessage Message { get; set; } = default!;
    public string ResponseBody { get; set; } = string.Empty;
    public IList<ApiError> Errors { get; set; } = new List<ApiError>();

    public ApiResponse ToApiResponse()
    {
        return new ApiResponse
        {
            Errors = Errors
        };
    }

    public static async Task<QuickResponse> FromMessage(HttpResponseMessage message)
    {
        var response = new QuickResponse
        {
            Message = message,
            ResponseBody = await message.Content.ReadAsStringAsync()
        };

        if (!message.IsSuccessStatusCode)
            response.HandleFailedCall();

        return response;
    }

    protected void HandleFailedCall()
    {
        try
        {
            Errors = JsonSerializer.Deserialize<List<ApiError>>(ResponseBody) ?? new List<ApiError>();

            if (!Errors.Any())
            {
                Errors.Add(new ApiError
                {
                    ErrorMessage = !string.IsNullOrEmpty(ResponseBody) ? ResponseBody : Message.StatusCode.ToString()
                });
            }
        }
        catch (Exception)
        {
            Errors.Add(new ApiError
            {
                ErrorMessage = !string.IsNullOrEmpty(ResponseBody) ? ResponseBody : Message.StatusCode.ToString()
            });
        }
    }
}

public class QuickResponse<T> : QuickResponse
{
    public T Data { get; set; } = default!;

    public new ApiResponse<T> ToApiResponse()
    {
        return new ApiResponse<T>
        {
            Errors = Errors,
            Data = Data
        };
    }

    public new static async Task<QuickResponse<T>> FromMessage(HttpResponseMessage message)
    {
        var response = new QuickResponse<T>
        {
            Message = message,
            ResponseBody = await message.Content.ReadAsStringAsync()
        };

        if (message.IsSuccessStatusCode)
        {
            try
            {
                response.Data = JsonSerializer.Deserialize<T>(response.ResponseBody)!;
            }
            catch (Exception)
            {
                response.HandleFailedCall();
            }
        }
        else
        {
            response.HandleFailedCall();
        }

        return response;
    }
}

public class QuickFile : QuickResponse<Stream>
{
    public new static async Task<QuickFile> FromMessage(HttpResponseMessage message)
    {
        var response = new QuickFile
        {
            Message = message,
            ResponseBody = await message.Content.ReadAsStringAsync()
        };

        if (message.IsSuccessStatusCode)
            response.Data = await message.Content.ReadAsStreamAsync();
        else
            response.HandleFailedCall();

        return response;
    }
}