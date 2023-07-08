using FluentEmailer.Core;
using FluentEmailer.Core.Interfaces;
using FluentEmailer.Core.Models;

namespace FluentEmailer.SendGrid;

public interface ISendGridSender : ISender
{
    /// <summary>
    /// SendGrid specific extension method that allows you to use a template instead of a message body.
    /// For more information, see: https://sendgrid.com/docs/ui/sending-email/how-to-send-an-email-with-dynamic-transactional-templates/.
    /// </summary>
    /// <param name="email">Fluent email.</param>
    /// <param name="templateId">SendGrid template ID.</param>
    /// <param name="templateData">SendGrid template data.</param>
    /// <param name="token">Optional cancellation token.</param>
    /// <returns>A SendResponse object.</returns>
    Task<SendResponse> SendWithTemplateAsync(IFluentEmailer email, string templateId, object templateData, CancellationToken token = default);
}