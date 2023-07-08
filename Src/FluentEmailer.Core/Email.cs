using FluentEmailer.Core.Defaults;
using FluentEmailer.Core.Interfaces;
using FluentEmailer.Core.Models;

namespace FluentEmailer.Core;

// ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
public class Email : IFluentEmailer
{
    public EmailData Data { get; set; }
    public ITemplateRenderer Renderer { get; set; }
    public ISender Sender { get; set; }

    public static ITemplateRenderer DefaultRenderer = new ReplaceRenderer();
    public static ISender DefaultSender = new SaveToDiskSender("/");

    /// <summary>
    /// Creates a new emailer instance with default settings.
    /// </summary>
    private Email() : this(DefaultRenderer, DefaultSender) { }

    /// <summary>
    ///  Creates a new Email instance with default settings, from a specific mailing address.
    /// </summary>
    /// <param name="emailAddress">Email address to send from</param>
    /// <param name="name">Name to send from</param>
    public Email(string emailAddress, string name = "")
        : this(DefaultRenderer, DefaultSender, emailAddress, name) { }

    /// <summary>
    ///  Creates a new Email instance using the given engines and mailing address.
    /// </summary>
    /// <param name="renderer">The template rendering engine</param>
    /// <param name="sender">The emailer sending implementation</param>
    /// <param name="emailAddress">Email address to send from</param>
    /// <param name="name">Name to send from</param>
    public Email(ITemplateRenderer renderer, ISender sender, string emailAddress = "", string name = "")
    {
        Data = new EmailData
        {
            FromAddress = new Address(emailAddress, name)
        };
        Renderer = renderer;
        Sender = sender;
    }

    /// <summary>
    /// Creates a new Email instance and sets the from property
    /// </summary>
    /// <param name="emailAddress">Email address to send from</param>
    /// <param name="name">Name to send from</param>
    /// <returns>Instance of the Email class</returns>
    public static IFluentEmailer From(string emailAddress, string name = "")
    {
        return new Email
        {
            Data = { FromAddress = new Address(emailAddress, name) }
        };
    }

    /// <inheritdoc />
    public IFluentEmailer SetFrom(string emailAddress, string? name = null)
    {
        Data.FromAddress = new Address(emailAddress, name);
        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer To(string emailAddress, string? name = null)
    {
        if (emailAddress.Contains(';'))
        {
            if (name is null)
            {
                foreach (string address in emailAddress.Split(';'))
                    Data.ToAddresses.Add(new Address(address));
            }
            else
            {
                var nameSplit = name.Split(';');
                var addressSplit = emailAddress.Split(';');
                if (nameSplit.Length != addressSplit.Length)
                    throw new ArgumentException($"To '{nameof(name)}' and '{nameof(emailAddress)}' arguments have different number of items.");

                for (int i = 0; i < addressSplit.Length; i++)
                {
                    var currentName = string.Empty;
                    if (nameSplit.Length - 1 >= i)
                        currentName = nameSplit[i];
                    Data.ToAddresses.Add(new Address(addressSplit[i].Trim(), currentName.Trim()));
                }
            }
        }
        else
        {
            Data.ToAddresses.Add(new Address(emailAddress.Trim(), name?.Trim()));
        }
        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer To(IEnumerable<Address> mailAddresses)
    {
        foreach (var address in mailAddresses)
            Data.ToAddresses.Add(address);
        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer CC(string emailAddress, string? name = null)
    {
        Data.CcAddresses.Add(new Address(emailAddress, name));
        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer CC(IEnumerable<Address> mailAddresses)
    {
        foreach (var address in mailAddresses)
            Data.CcAddresses.Add(address);
        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer BCC(string emailAddress, string? name = null)
    {
        Data.BccAddresses.Add(new Address(emailAddress, name));
        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer BCC(IEnumerable<Address> mailAddresses)
    {
        foreach (var address in mailAddresses)
            Data.BccAddresses.Add(address);
        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer ReplyTo(string address, string? name = null)
    {
        Data.ReplyToAddresses.Add(new Address(address, name));
        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer Subject(string subject)
    {
        Data.Subject = subject;
        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer Body(string body, bool isHtml = false)
    {
        Data.IsHtml = isHtml;
        Data.Body = body;
        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer PlaintextAlternativeBody(string body)
    {
        Data.PlaintextAlternativeBody = body;
        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer HighPriority()
    {
        Data.Priority = Priority.High;
        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer LowPriority()
    {
        Data.Priority = Priority.Low;
        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer UsingTemplateEngine(ITemplateRenderer renderer)
    {
        Renderer = renderer;
        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer UsingTemplateFromEmbedded<T>(string path, T model, Assembly assembly, bool isHtml = true)
        where T : notnull
    {
        var template = EmbeddedResourceHelper.GetResourceAsString(assembly, path);
        var result = Renderer.Parse(template, model, isHtml);
        Data.IsHtml = isHtml;
        Data.Body = result;

        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer PlaintextAlternativeUsingTemplateFromEmbedded<T>(string path, T model, Assembly assembly)
        where T : notnull
    {
        var template = EmbeddedResourceHelper.GetResourceAsString(assembly, path);
        var result = Renderer.Parse(template, model, false);
        Data.PlaintextAlternativeBody = result;

        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer UsingTemplateFromFile<T>(string filename, T model, bool isHtml = true)
        where T : notnull
    {
        string template;

        using (var reader = new StreamReader(File.OpenRead(filename)))
            template = reader.ReadToEnd();

        var result = Renderer.Parse(template, model, isHtml);
        Data.IsHtml = isHtml;
        Data.Body = result;

        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer PlaintextAlternativeUsingTemplateFromFile<T>(string filename, T model)
        where T : notnull
    {
        string template;

        using (var reader = new StreamReader(File.OpenRead(filename)))
            template = reader.ReadToEnd();

        var result = Renderer.Parse(template, model, false);
        Data.PlaintextAlternativeBody = result;

        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer UsingCultureTemplateFromFile<T>(string filename, T model, CultureInfo culture, bool isHtml = true)
        where T : notnull
    {
        var cultureFile = GetCultureFileName(filename, culture);
        return UsingTemplateFromFile(cultureFile, model, isHtml);
    }

    /// <inheritdoc />
    public IFluentEmailer PlaintextAlternativeUsingCultureTemplateFromFile<T>(string filename, T model, CultureInfo culture)
        where T : notnull
    {
        var cultureFile = GetCultureFileName(filename, culture);
        return PlaintextAlternativeUsingTemplateFromFile(cultureFile, model);
    }

    /// <inheritdoc />
    public IFluentEmailer UsingTemplate<T>(string template, T model, bool isHtml = true)
        where T : notnull
    {
        var result = Renderer.Parse(template, model, isHtml);
        Data.IsHtml = isHtml;
        Data.Body = result;

        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer PlaintextAlternativeUsingTemplate<T>(string template, T model)
        where T : notnull
    {
        var result = Renderer.Parse(template, model, false);
        Data.PlaintextAlternativeBody = result;

        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer Attach(Attachment attachment)
    {
        if (!Data.Attachments.Contains(attachment))
            Data.Attachments.Add(attachment);
        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer Attach(IEnumerable<Attachment> attachments)
    {
        foreach (var attachment in attachments.Where(attachment => !Data.Attachments.Contains(attachment)))
            Data.Attachments.Add(attachment);
        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer AttachFromFilename(string filename, string contentType = "", string? attachmentName = null)
    {
        var stream = File.OpenRead(filename);
        Attach(new Attachment
        {
            Data = stream,
            Filename = attachmentName ?? filename,
            ContentType = contentType
        });

        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer Tag(string tag)
    {
        Data.Tags.Add(tag);
        return this;
    }

    /// <inheritdoc />
    public IFluentEmailer Header(string header, string body)
    {
        Data.Headers.Add(header, body);
        return this;
    }

    /// <inheritdoc />
    public virtual SendResponse Send(CancellationToken token = default) 
        => Sender.Send(this, token);

    /// <inheritdoc />
    public virtual Task<SendResponse> SendAsync(CancellationToken token = default) 
        => Sender.SendAsync(this, token);

    private static string GetCultureFileName(string fileName, CultureInfo culture)
    {
        var extension = Path.GetExtension(fileName);
        var cultureExtension = $"{culture.Name}{extension}";

        var cultureFile = Path.ChangeExtension(fileName, cultureExtension);
        return File.Exists(cultureFile) ? cultureFile : fileName;
    }
}
