using FluentEmailer.Core;
using FluentEmailer.Core.Models;

namespace FluentEmailer.SendGrid;

public static class IFluentEmailerExtensions
{
    public static async Task<SendResponse> SendWithTemplateAsync(this IFluentEmailer email, string templateId, object templateData)
    {
        var sendGridSender = email.Sender as ISendGridSender;
        return await sendGridSender!.SendWithTemplateAsync(email, templateId, templateData);
    }
}