using FluentEmailer.Core.Models;

namespace FluentEmailer.Core.Interfaces;

public interface ISender
{
    SendResponse Send(IFluentEmailer emailer, CancellationToken token = default);
    Task<SendResponse> SendAsync(IFluentEmailer emailer, CancellationToken token = default);
}
