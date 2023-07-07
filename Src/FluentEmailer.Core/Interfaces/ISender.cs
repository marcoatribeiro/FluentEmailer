using FluentEmailer.Core.Models;

namespace FluentEmailer.Core.Interfaces;

public interface ISender
{
    SendResponse Send(IFluentEmail email, CancellationToken? token = null);
    Task<SendResponse> SendAsync(IFluentEmail email, CancellationToken? token = null);
}
