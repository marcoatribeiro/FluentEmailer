using FluentEmailer.Core.Interfaces;
using FluentEmailer.Core.Models;

namespace FluentEmailer.Core;

public interface IFluentEmailer: IHideObjectMembers
{
    EmailData Data { get; set; }
    ITemplateRenderer Renderer { get; set; }
    ISender Sender { get; set; }

    /// <summary>
    /// Set the send from emailer address
    /// </summary>
    /// <param name="emailAddress">Email address of sender</param>
    /// <param name="name">Name of sender</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer SetFrom(string emailAddress, string? name = null);

    /// <summary>
    /// Adds one or more TO recipient(s) to the emailer. You can use ";" to include more than one recipient.
    /// </summary>
    /// <param name="emailAddress">Email address of recipient(s).</param>
    /// <param name="name">Name of recipient(s).</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer To(string emailAddress, string? name = null);

    /// <summary>
    /// Adds all recipients in list to emailer
    /// </summary>
    /// <param name="mailAddresses">List of recipients</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer To(IEnumerable<Address> mailAddresses);

    /// <summary>
    /// Adds a Carbon Copy recipient to the emailer.
    /// </summary>
    /// <param name="emailAddress">Email address to cc</param>
    /// <param name="name">Name to cc</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer CC(string emailAddress, string? name = null);

    /// <summary>
    /// Adds all Carbon Copy in list to an emailer
    /// </summary>
    /// <param name="mailAddresses">List of recipients to CC</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer CC(IEnumerable<Address> mailAddresses);

    /// <summary>
    /// Adds a blind carbon copy to the emailer
    /// </summary>
    /// <param name="emailAddress">Email address of bcc</param>
    /// <param name="name">Name of bcc</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer BCC(string emailAddress, string? name = null);

    /// <summary>
    /// Adds all blind carbon copy in list to an emailer
    /// </summary>
    /// <param name="mailAddresses">List of recipients to BCC</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer BCC(IEnumerable<Address> mailAddresses);

    /// <summary>
    /// Sets the ReplyTo address on the emailer
    /// </summary>
    /// <param name="address">The ReplyTo Address</param>
    /// <param name="name">The Display Name of the ReplyTo</param>
    /// <returns></returns>
    IFluentEmailer ReplyTo(string address, string? name = null);

    /// <summary>
    /// Sets the subject of the emailer
    /// </summary>
    /// <param name="subject">emailer subject</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer Subject(string subject);

    /// <summary>
    /// Adds a Body to the Email
    /// </summary>
    /// <param name="body">The content of the body</param>
    /// <param name="isHtml">True if Body is HTML, false for plain text (Optional)</param>
    IFluentEmailer Body(string body, bool isHtml = false);

    /// <summary>
    /// Marks the emailer as High Priority
    /// </summary>
    IFluentEmailer HighPriority();

    /// <summary>
    /// Marks the emailer as Low Priority
    /// </summary>
    IFluentEmailer LowPriority();

    /// <summary>
    /// Set the template rendering engine to use, defaults to RazorEngine
    /// </summary>
    IFluentEmailer UsingTemplateEngine(ITemplateRenderer renderer);

    /// <summary>
    /// Adds template to emailer from embedded resource
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">Path the the embedded resource eg [YourAssembly].[YourResourceFolder].[YourFilename.txt]</param>
    /// <param name="model">Model for the template</param>
    /// <param name="assembly">The assembly your resource is in. Defaults to calling assembly.</param>
    /// <param name="isHtml">True if Body is HTML, false for plain text (Optional)</param>
    /// <returns></returns>
    IFluentEmailer UsingTemplateFromEmbedded<T>(string path, T model, Assembly assembly, bool isHtml = true)
        where T : notnull;

    /// <summary>
    /// Adds the template file to the emailer
    /// </summary>
    /// <param name="filename">The path to the file to load</param>
    /// <param name="model">Model for the template</param>
    /// <param name="isHtml">True if Body is HTML, false for plain text (Optional)</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer UsingTemplateFromFile<T>(string filename, T model, bool isHtml = true)
        where T : notnull;

    /// <summary>
    /// Adds a culture specific template file to the emailer
    /// </summary>
    /// <param name="filename">The path to the file to load</param>
    /// /// <param name="model">The razor model</param>
    /// <param name="culture">The culture of the template (Default is the current culture)</param>
    /// <param name="isHtml">True if Body is HTML, false for plain text (Optional)</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer UsingCultureTemplateFromFile<T>(string filename, T model, CultureInfo culture, bool isHtml = true)
        where T : notnull;

    /// <summary>
    /// Adds razor template to the emailer
    /// </summary>
    /// <param name="template">The razor template</param>
    /// <param name="model">Model for the template</param>
    /// <param name="isHtml">True if Body is HTML, false for plain text (Optional)</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer UsingTemplate<T>(string template, T model, bool isHtml = true)
        where T : notnull;

    /// <summary>
    /// Adds an Attachment to the Email
    /// </summary>
    /// <param name="attachment">The Attachment to add</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer Attach(Attachment attachment);

    /// <summary>
    /// Adds Multiple Attachments to the Email
    /// </summary>
    /// <param name="attachments">The List of Attachments to add</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer Attach(IEnumerable<Attachment> attachments);

    /// <summary>
    /// Sends emailer synchronously
    /// </summary>
    /// <returns>Instance of the Email class</returns>
    SendResponse Send(CancellationToken token = default);

    /// <summary>
    /// Sends emailer asynchronously
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    Task<SendResponse> SendAsync(CancellationToken token = default);

    IFluentEmailer AttachFromFilename(string filename, string contentType = "", string? attachmentName = null);

    /// <summary>
    /// Adds a Plaintext alternative Body to the Email. Used in conjunction with an HTML emailer,
    /// this allows for emailer readers without html capability, and also helps avoid spam filters.
    /// </summary>
    /// <param name="body">The content of the body</param>
    IFluentEmailer PlaintextAlternativeBody(string body);

    /// <summary>
    /// Adds template to emailer from embedded resource
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">Path the the embedded resource eg [YourAssembly].[YourResourceFolder].[YourFilename.txt]</param>
    /// <param name="model">Model for the template</param>
    /// <param name="assembly">The assembly your resource is in. Defaults to calling assembly.</param>
    /// <returns></returns>
    IFluentEmailer PlaintextAlternativeUsingTemplateFromEmbedded<T>(string path, T model, Assembly assembly)
        where T : notnull;

    /// <summary>
    /// Adds the template file to the emailer
    /// </summary>
    /// <param name="filename">The path to the file to load</param>
    /// <param name="model">Model for the template</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer PlaintextAlternativeUsingTemplateFromFile<T>(string filename, T model)
        where T : notnull;

    /// <summary>
    /// Adds a culture specific template file to the emailer
    /// </summary>
    /// <param name="filename">The path to the file to load</param>
    /// /// <param name="model">The razor model</param>
    /// <param name="culture">The culture of the template (Default is the current culture)</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer PlaintextAlternativeUsingCultureTemplateFromFile<T>(string filename, T model, CultureInfo culture)
        where T : notnull;

    /// <summary>
    /// Adds razor template to the emailer
    /// </summary>
    /// <param name="template">The razor template</param>
    /// <param name="model">Model for the template</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer PlaintextAlternativeUsingTemplate<T>(string template, T model)
        where T : notnull;

    /// <summary>
    /// Adds tag to the Email. This is currently only supported by the Mailgun provider. <see href="https://documentation.mailgun.com/en/latest/user_manual.html#tagging"/>
    /// </summary>
    /// <param name="tag">Tag name, max 128 characters, ASCII only</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer Tag(string tag);

    /// <summary>
    /// Adds header to the Email.
    /// </summary>
    /// <param name="header">Header name, only printable ASCII allowed.</param>
    /// <param name="body">value of the header</param>
    /// <returns>Instance of the Email class</returns>
    IFluentEmailer Header(string header, string body);
}