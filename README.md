# FluentEmailer - Email sender for .NET
This library is based on the excellent [FluentEmail](https://github.com/lukencode/FluentEmail) by Luke Lowrey, which, at the time I'm writing this, looks like it is not being maintained any longer (unfortunately).

> Note: This library is not compatible with .NET Framework projects.


[![NuGet](https://img.shields.io/nuget/v/FluentEmailer.Core.svg)](https://nuget.org/packages/FluentEmailer.Core)
[![MIT](https://img.shields.io/github/license/marcoatribeiro/FluentEmailer)](https://github.com/marcoatribeiro/FluentEmailer/LICENSE)

## Nuget Packages

### Core Library

* [FluentEmailer.Core](src/FluentEmailer.Core) - Just the domain model. Includes very basic defaults, but is also included with every other package here.
* [FluentEmailer.Smtp](src/Senders/FluentEmailer.Smtp) - Send email via SMTP server.

### Renderers

* [FluentEmailer.Razor](src/Renderers/FluentEmailer.Razor) - Generate emails using Razor templates. Anything you can do in ASP.NET is possible here. Uses the [RazorLight](https://github.com/toddams/RazorLight) project under the hood. 
* [FluentEmailer.Liquid](src/Renderers/FluentEmailer.Liquid) - Generate emails using [Liquid templates](https://shopify.github.io/liquid/). Uses the [Fluid](https://github.com/sebastienros/fluid) project under the hood. 

### Mail Provider Integrations

* [FluentEmailer.MailerSend](src/Senders/FluentEmailer.MailerSend) - Send email via [MailerSend](https://www.mailersend.com/)'s REST API.
* [FluentEmailer.Mailgun](src/Senders/FluentEmailer.Mailgun) - Send emails via [Mailgun](https://www.mailgun.com/)'s REST API.
* [FluentEmailer.SendGrid](src/Senders/FluentEmailer.SendGrid) - Send email via [SendGrid](https://docs.sendgrid.com/for-developers/sending-email/api-getting-started)'s REST API.


## Basic Usage
```csharp
var email = await Email
    .From("john@email.com")
    .To("bob@email.com", "bob")
    .Subject("hows it going bob")
    .Body("yo bob, long time no see!")
    .SendAsync();
```


## Dependency Injection

Configure FluentEmailer in startup.cs with these helper methods. This will inject `IFluentEmailer` (send a single email) and `IFluentEmailerFactory` (used to send multiple emails in a single context) with the 
ISender and ITemplateRenderer configured using AddLiquidRenderer(), AddSmtpSender() or other packages.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddFluentEmail("fromemail@test.test")
        .AddRazorRenderer()
        .AddSmtpSender("localhost", 25);
}
```
Example to take a dependency on IFluentEmailer:

```c#
public class EmailService {

   private IFluentEmailer _fluentEmailer;

   public EmailService(IFluentEmailer fluentEmailer) {
     _fluentEmailer = fluentEmailer;
   }

   public async Task Send() {
     await _fluentEmailer.To("hellO@gmail.com")
          .Body("The body").SendAsync();
   }
}
```


## Using a Razor template

```csharp
// Using Razor templating package (or set using AddRazorRenderer in services)
Email.DefaultRenderer = new RazorRenderer();

var template = "Dear @Model.Name, You are totally @Model.Compliment.";

var email = Email
    .From("bob@hotmail.com")
    .To("somedude@gmail.com")
    .Subject("woo nuget")
    .UsingTemplate(template, new { Name = "Luke", Compliment = "Awesome" });
```


## Using a Liquid template

[Liquid templates](https://shopify.github.io/liquid/) are a more secure option for Razor templates as they run in more restricted environment.
While Razor templates have access to whole power of CLR functionality like file access, they also
are more insecure if templates come from untrusted source. Liquid templates also have the benefit of being faster
to parse initially as they don't need heavy compilation step like Razor templates do.

Model properties are exposed directly as properties in Liquid templates so they also become more compact.

See [Fluid samples](https://github.com/sebastienros/fluid) for more examples.

```csharp
// Using Liquid templating package (or set using AddLiquidRenderer in services)

// file provider is used to resolve layout files if they are in use
var fileProvider = new PhysicalFileProvider(Path.Combine(someRootPath, "EmailTemplates"));
var options = new LiquidRendererOptions
{
    FileProvider = fileProvider
};

Email.DefaultRenderer = new LiquidRenderer(Options.Create(options));

// template which utilizes layout
var template = @"
{% layout '_layout.liquid' %}
Dear {{ Name }}, You are totally {{ Compliment }}.";

var email = Email
    .From("bob@hotmail.com")
    .To("somedude@gmail.com")
    .Subject("woo nuget")
    .UsingTemplate(template, new ViewModel { Name = "Luke", Compliment = "Awesome" });
```

## Sending Emails

```csharp
// Using Smtp Sender package (or set using AddSmtpSender in services)
Email.DefaultSender = new SmtpSender();

//send normally
email.Send();

//send asynchronously
await email.SendAsync();
```

## Template File from Disk

```csharp
var email = Email
    .From("bob@hotmail.com")
    .To("somedude@gmail.com")
    .Subject("woo nuget")
    .UsingTemplateFromFile($"{Directory.GetCurrentDirectory()}/Mytemplate.cshtml", new { Name = "Rad Dude" });
```

## Embedded Template File

```csharp
var email = new Email("bob@hotmail.com")
	.To("benwholikesbeer@twitter.com")
	.Subject("Hey cool name!")
	.UsingTemplateFromEmbedded("Example.Project.Namespace.template-name.cshtml", 
		new { Name = "Bob" }, 
		TypeFromYourEmbeddedAssembly.GetType().GetTypeInfo().Assembly);
```

