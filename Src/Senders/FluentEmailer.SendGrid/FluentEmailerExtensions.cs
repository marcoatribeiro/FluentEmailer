using FluentEmailer.Core;
using FluentEmailer.Core.Models;

namespace FluentEmailer.SendGrid;

public static class FluentEmailerExtensions
{
    /// <summary>
    /// Adds support for SendGrid transactional templates. <see href="https://github.com/sendgrid/sendgrid-csharp/blob/master/USE_CASES.md#with-mail-helper-class"/>
    /// </summary>
    public static async Task<SendResponse> SendWithTemplateAsync(this IFluentEmailer email, string templateId, object templateData)
    {
        var sendGridSender = email.Sender as ISendGridSender;
        return await sendGridSender!.SendWithTemplateAsync(email, templateId, templateData);
    }
}